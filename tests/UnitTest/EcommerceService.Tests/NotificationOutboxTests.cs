using Contracts.Application.Common.Interfaces.Services.Notifications;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Infrastructure.Data;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Notification_Grpc;
using Npgsql;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace EcommerceService.Tests;

public class NotificationOutboxTests
{
    [Fact]
    public void OnlyRealProcessedTransitionsWithCustomersProduceAnImmutableIntent()
    {
        var order = new Order(2, 7, "OD-1001", 100, 110, OrderStatus.InProgress, customerId: 501) { Id = 1001 };
        Assert.Null(NotificationOutbox.FromOrder(order));
        order.TransitionTo(OrderStatus.Processed);
        var message = NotificationOutbox.FromOrder(order)!;
        Assert.Equal("order-processed:1001", message.Id);
        Assert.Contains("OD-1001", message.Payload);
        order.Code = "changed";
        Assert.DoesNotContain("changed", message.Payload);
        order.CustomerId = null;
        Assert.Null(NotificationOutbox.FromOrder(order));
        var seed = new Order(2, 7, "SEED", 100, 110, OrderStatus.Processed, customerId: 501);
        Assert.Null(NotificationOutbox.FromOrder(seed));
    }

    [DevelopmentSeedDatabaseFact]
    public async Task FailureRetriesWithSameIdentity_ThenMarksDurableAcknowledgement()
    {
        await using var fixture = await Fixture.Create();
        var requests = new List<SendNotificationRequest>();
        var transport = new Mock<INotificationGrpc>();
        transport.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .Returns((SendNotificationRequest request, CancellationToken _) =>
            {
                requests.Add(request.Clone());
                return Task.FromResult(requests.Count > 1);
            });
        await fixture.Dispatch(transport.Object);
        await using var db = fixture.Context();
        var failed = await db.Set<NotificationOutbox>().SingleAsync();
        Assert.Null(failed.DeliveredAt);
        Assert.Null(failed.LockedUntil);
        Assert.Equal(1, failed.Attempts);
        Assert.True(failed.NextAttemptAt > failed.CreatedAt);
        Assert.Equal(nameof(InvalidOperationException), failed.LastError);
        Assert.False(await fixture.Dispatch(transport.Object));
        await fixture.MakeDue();
        Assert.True(await fixture.Dispatch(transport.Object));
        db.ChangeTracker.Clear();
        var delivered = await db.Set<NotificationOutbox>().SingleAsync();
        Assert.NotNull(delivered.DeliveredAt);
        Assert.Null(delivered.LastError);
        Assert.Equal(2, delivered.Attempts);
        Assert.Equal(requests[0].MessageId, requests[1].MessageId);
        Assert.Equal(requests[0].Time, requests[1].Time);
        Assert.Equal("#2", requests[0].Parameters["branch_name"]);
        Assert.Equal("501", Assert.Single(requests[0].UserIds));
        Assert.False(await fixture.Dispatch(transport.Object));
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ConcurrentWorkersOnlyClaimOnce_ExpiredCrashLeaseIsRecoverable()
    {
        await using var fixture = await Fixture.Create();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new Mock<INotificationGrpc>();
        transport.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .Returns((SendNotificationRequest _, CancellationToken _) => { entered.SetResult(); return release.Task; });
        var first = fixture.Dispatch(transport.Object);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(await fixture.Dispatch(transport.Object));
        release.SetResult(true);
        Assert.True(await first);
        transport.Verify(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        await using var db = fixture.Context();
        await db.Set<NotificationOutbox>().ExecuteUpdateAsync(s => s.SetProperty(x => x.DeliveredAt, (DateTimeOffset?)null)
            .SetProperty(x => x.LockedUntil, DateTimeOffset.UtcNow.AddMinutes(-1)));
        var recovery = new Mock<INotificationGrpc>();
        recovery.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Assert.True(await fixture.Dispatch(recovery.Object));
        recovery.Verify(x => x.SendNotifyAsync(It.Is<SendNotificationRequest>(r => r.MessageId == "order-processed:1001"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ShutdownAndTransportExceptionsNeverDiscardPendingMessages()
    {
        await using var fixture = await Fixture.Create();
        using var cancellation = new CancellationTokenSource();
        var transport = new Mock<INotificationGrpc>();
        transport.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .Returns((SendNotificationRequest _, CancellationToken token) =>
            {
                cancellation.Cancel();
                return Task.FromCanceled<bool>(token);
            });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => fixture.Dispatch(transport.Object, cancellation.Token));
        await using var db = fixture.Context();
        Assert.Null((await db.Set<NotificationOutbox>().SingleAsync()).DeliveredAt);
        await fixture.MakeDue();
        transport.Reset();
        transport.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("transport down"));
        Assert.True(await fixture.Dispatch(transport.Object));
        db.ChangeTracker.Clear();
        var pending = await db.Set<NotificationOutbox>().SingleAsync();
        Assert.Null(pending.DeliveredAt);
        Assert.Equal(nameof(IOException), pending.LastError);
        Assert.Equal(2, pending.Attempts);
    }

    private sealed class Fixture(NpgsqlDataSource source) : IAsyncDisposable
    {
        public TheDbContext Context() => new(new DbContextOptionsBuilder<TheDbContext>().UseNpgsql(source).Options);
        public static async Task<Fixture> Create()
        {
            var connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
            var builder = new NpgsqlConnectionStringBuilder(connection);
            Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
            Assert.StartsWith("vietwash_seed_test", builder.Database);
            var schema = "outbox_" + Guid.NewGuid().ToString("N");
            await using var admin = new NpgsqlConnection(connection);
            await admin.OpenAsync();
            await new NpgsqlCommand($"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}", admin).ExecuteNonQueryAsync();
            builder.SearchPath = $"{schema},public";
            var fixture = new Fixture(new NpgsqlDataSourceBuilder(builder.ConnectionString).EnableDynamicJson().Build());
            await using var db = fixture.Context();
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            var order = new Order(2, 7, "OD-1001", 100, 110, OrderStatus.InProgress, customerId: 501) { Id = 1001 };
            order.TransitionTo(OrderStatus.Processed);
            db.Set<NotificationOutbox>().Add(NotificationOutbox.FromOrder(order)!);
            await db.SaveChangesAsync();
            return fixture;
        }
        public async Task<bool> Dispatch(INotificationGrpc transport, CancellationToken token = default)
        {
            await using var db = Context();
            return await new NotificationOutboxDispatcher(db, transport, Log.Logger).DispatchOneAsync(token);
        }
        public async Task MakeDue()
        {
            await using var db = Context();
            await db.Set<NotificationOutbox>().ExecuteUpdateAsync(s => s
                .SetProperty(x => x.NextAttemptAt, DateTimeOffset.UtcNow.AddMinutes(-1))
                .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null));
        }
        public ValueTask DisposeAsync() => source.DisposeAsync();
    }
}

internal sealed class NotificationLogSink : ILogEventSink
{
    public List<LogEvent> Events { get; } = [];
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}
