using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Common.Projections;
using Domain.Aggregates.Notifications;
using Infrastructure.Data;
using Infrastructure.Services.Notifications;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Npgsql;
using Serilog;

namespace NotificationService.Tests;

public sealed class NotificationDatabaseFactAttribute : FactAttribute
{
    public NotificationDatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")))
            Skip = "Set VIETWASH_SEED_TEST_DATABASE to a disposable local PostgreSQL database.";
    }
}

public class NotificationInboxTests
{
    [NotificationDatabaseFact]
    public async Task ConcurrentDuplicateDeliveryCreatesOneInboxItem_OnlyAfterCommit()
    {
        await using var fixture = await Fixture.Create();
        var pushes = 0;
        var client = new Mock<IClientProxy>();
        client.Setup(x => x.SendCoreAsync("ReceiveNotification", It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await using var verification = fixture.Context();
                Assert.Equal(1, await verification.Set<Notification>().CountAsync());
                Assert.Equal(1, await verification.Set<NotificationReceipt>().CountAsync());
                Interlocked.Increment(ref pushes);
            });
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => fixture.Send(Request(), client.Object)));
        await using var db = fixture.Context();
        Assert.Single(await db.Set<Notification>().ToListAsync());
        Assert.Single(await db.Set<NotificationReceipt>().ToListAsync());
        Assert.Equal(1, pushes);
        // Deleting a UI notification must not delete the receipt or allow a late replay.
        await db.Set<Notification>().ExecuteDeleteAsync();
        await fixture.Send(Request(), client.Object);
        Assert.Empty(await db.Set<Notification>().ToListAsync());
        Assert.Equal(1, pushes);
    }

    [NotificationDatabaseFact]
    public async Task OfflineClientDoesNotLoseDurableNotification_AndReplayIsAcknowledged()
    {
        await using var fixture = await Fixture.Create();
        var offline = new Mock<IClientProxy>();
        offline.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("browser disconnected"));
        await fixture.Send(Request(), offline.Object);
        await fixture.Send(Request(), offline.Object);
        await using var db = fixture.Context();
        var item = Assert.Single(await db.Set<Notification>().ToListAsync());
        Assert.False(item.IsRead);
        Assert.Equal("Done OD-1", item.Content);
        offline.Verify(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [NotificationDatabaseFact]
    public async Task FailedPersistenceRollsBackReceipt_ThenCanRetry()
    {
        await using var fixture = await Fixture.Create();
        var request = Request();
        request.TemplateId = "missing";
        var socket = new Mock<IClientProxy>(MockBehavior.Strict);
        await Assert.ThrowsAsync<Exception>(() => fixture.Send(request, socket.Object));
        await using var db = fixture.Context();
        Assert.Empty(await db.Set<NotificationReceipt>().ToListAsync());
        Assert.Empty(await db.Set<Notification>().ToListAsync());
        socket.VerifyNoOtherCalls();
        await fixture.Send(Request(), Mock.Of<IClientProxy>());
        Assert.Single(await db.Set<NotificationReceipt>().ToListAsync());
        Assert.Single(await db.Set<Notification>().ToListAsync());
    }

    [NotificationDatabaseFact]
    public async Task MessageIdentityCannotBeReusedForDifferentRecipients()
    {
        await using var fixture = await Fixture.Create();
        await fixture.Send(Request(), Mock.Of<IClientProxy>());
        var changed = Request();
        changed.UserIds = ["another-user"];
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Send(changed, Mock.Of<IClientProxy>()));
        await using var db = fixture.Context();
        Assert.Equal("501", Assert.Single(await db.Set<Notification>().ToListAsync()).UserId);
    }

    [NotificationDatabaseFact]
    public async Task LegacyProducerWithoutMessageIdStillPersistsBeforeSending()
    {
        await using var fixture = await Fixture.Create();
        var request = Request();
        request.MessageId = null;
        await fixture.Send(request, Mock.Of<IClientProxy>());
        await using var db = fixture.Context();
        Assert.Single(await db.Set<Notification>().ToListAsync());
        Assert.Empty(await db.Set<NotificationReceipt>().ToListAsync());
    }

    private static NotificationModel Request() => new()
    {
        MessageId = "order-processed:1001", TemplateId = "laundry_processed", UserIds = ["501", "501"],
        Parameters = new() { ["order_code"] = "OD-1" }, Data = new() { ["order_id"] = "1001" },
        Time = "2026-09-03T00:00:00.0000000+00:00",
    };

    private sealed class Fixture(NpgsqlDataSource source) : IAsyncDisposable
    {
        public TheDbContext Context() => new(new DbContextOptionsBuilder<TheDbContext>().UseNpgsql(source).Options);
        public static async Task<Fixture> Create()
        {
            var connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
            var builder = new NpgsqlConnectionStringBuilder(connection);
            Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
            Assert.StartsWith("vietwash_seed_test", builder.Database);
            var schema = "inbox_" + Guid.NewGuid().ToString("N");
            await using var admin = new NpgsqlConnection(connection);
            await admin.OpenAsync();
            await new NpgsqlCommand($"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE EXTENSION IF NOT EXISTS hstore WITH SCHEMA public; CREATE SCHEMA {schema}", admin).ExecuteNonQueryAsync();
            builder.SearchPath = $"{schema},public";
            var fixture = new Fixture(new NpgsqlDataSourceBuilder(builder.ConnectionString).EnableDynamicJson().Build());
            await using var db = fixture.Context();
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            db.Set<NotificationTemplate>().Add(new() { Id = "laundry_processed", Title = "Laundry done", Content = "Done {{order_code}}", ContentHtml = "<p>Done {{order_code}}</p>" });
            await db.SaveChangesAsync();
            return fixture;
        }
        public async Task Send(NotificationModel request, IClientProxy client)
        {
            await using var db = Context();
            var hub = new Mock<IHubContext<NotificationHub>>();
            hub.Setup(x => x.Clients.Group(It.IsAny<string>())).Returns(client);
            await new Infrastructure.Services.Notifications.NotificationService(Mock.Of<IUnitOfWork>(), hub.Object, db, Log.Logger)
                .SendAsync(request, CancellationToken.None);
        }
        public ValueTask DisposeAsync() => source.DisposeAsync();
    }
}
