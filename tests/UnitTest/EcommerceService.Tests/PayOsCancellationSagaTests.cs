using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Users;
using Infrastructure.Data;
using Infrastructure.Payments;
using Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Net.payOS.Types;
using Npgsql;
using Serilog;

namespace EcommerceService.Tests;

public class PayOsCancellationSagaTests
{
    [DevelopmentSeedDatabaseFact]
    public async Task ProviderRunsOnlyAfterPendingAndClaimTransactionsCommit()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);

        await using (var verification = fixture.Context())
        {
            Assert.Equal(
                PayOsCancellationState.Pending,
                (await verification.Set<PayOsCancellationRequest>().SingleAsync()).State
            );
            Assert.Equal(OrderStatus.Processed, (await verification.Set<Order>().SingleAsync()).Status);
        }

        var client = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        client.Setup(x => x.GetPaymentLinkInformationAsync(1001)).Returns(async () =>
        {
            await using var duringProviderCall = fixture.Context();
            Assert.Equal(
                PayOsCancellationState.Processing,
                (await duringProviderCall.Set<PayOsCancellationRequest>().SingleAsync()).State
            );
            Assert.Equal(
                OrderStatus.Processed,
                (await duringProviderCall.Set<Order>().SingleAsync()).Status
            );
            return Link("CANCELLED");
        });

        Assert.True(await fixture.DispatchAsync(client.Object));
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ProviderFailureRemainsRetryableAndCanCompleteLater()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);
        var failedClient = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        failedClient
            .Setup(x => x.GetPaymentLinkInformationAsync(1001))
            .ThrowsAsync(new HttpRequestException("Temporary provider failure"));

        Assert.True(await fixture.DispatchAsync(failedClient.Object));
        await using (var failedCheck = fixture.Context())
        {
            PayOsCancellationRequest request = await failedCheck
                .Set<PayOsCancellationRequest>()
                .SingleAsync();
            Assert.Equal(PayOsCancellationState.Failed, request.State);
            Assert.NotNull(request.NextAttemptAt);
            Assert.Equal(1, request.Attempts);
            Assert.Equal(OrderStatus.Processed, (await failedCheck.Set<Order>().SingleAsync()).Status);
        }

        await fixture.MakeDueAsync();
        var retryClient = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        retryClient
            .Setup(x => x.GetPaymentLinkInformationAsync(1001))
            .ReturnsAsync(Link("CANCELLED"));
        Assert.True(await fixture.DispatchAsync(retryClient.Object));

        await using var completedCheck = fixture.Context();
        Assert.Equal(
            PayOsCancellationState.Completed,
            (await completedCheck.Set<PayOsCancellationRequest>().SingleAsync()).State
        );
        Assert.Equal(OrderStatus.Cancelled, (await completedCheck.Set<Order>().SingleAsync()).Status);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task PendingPaymentIsCancelledAndOrderIsFinalized()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);
        var client = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        client.Setup(x => x.GetPaymentLinkInformationAsync(1001)).ReturnsAsync(Link("PENDING"));
        client
            .Setup(x => x.CancelPaymentLinkAsync(1001, "Customer request"))
            .ReturnsAsync(Link("CANCELLED"));

        Assert.True(await fixture.DispatchAsync(client.Object));

        await using var db = fixture.Context();
        Order order = await db.Set<Order>().SingleAsync();
        PayOsCancellationRequest request = await db.Set<PayOsCancellationRequest>().SingleAsync();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal("Customer request", order.CancellationReason);
        Assert.Equal(7, order.CancelledBy);
        Assert.Equal(PayOsCancellationState.Completed, request.State);
        Assert.Equal("Cancelled", request.ProviderState);
        client.Verify(x => x.CancelPaymentLinkAsync(1001, "Customer request"), Times.Once);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task AlreadyCancelledIsIdempotentSuccessWithoutAnotherCancelCall()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);
        var client = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        client
            .Setup(x => x.GetPaymentLinkInformationAsync(1001))
            .ReturnsAsync(Link("CANCELLED"));

        Assert.True(await fixture.DispatchAsync(client.Object));

        await using var db = fixture.Context();
        Assert.Equal(OrderStatus.Cancelled, (await db.Set<Order>().SingleAsync()).Status);
        Assert.Equal(
            PayOsCancellationState.Completed,
            (await db.Set<PayOsCancellationRequest>().SingleAsync()).State
        );
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [DevelopmentSeedDatabaseFact]
    public async Task PaidPaymentFailsTerminallyAndNeverCancelsOrder()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);
        var client = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        client.Setup(x => x.GetPaymentLinkInformationAsync(1001)).ReturnsAsync(Link("PAID"));

        Assert.True(await fixture.DispatchAsync(client.Object));

        await using var db = fixture.Context();
        Assert.Equal(OrderStatus.Processed, (await db.Set<Order>().SingleAsync()).Status);
        PayOsCancellationRequest request = await db.Set<PayOsCancellationRequest>().SingleAsync();
        Assert.Equal(PayOsCancellationState.Failed, request.State);
        Assert.Null(request.NextAttemptAt);
        Assert.Equal("Paid", request.ProviderState);
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ExpiredCrashLeaseRecoversWithoutRepeatingProviderCancellation()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);
        await using (var crashedAttempt = fixture.Context())
        {
            PayOsCancellationRequest request = await crashedAttempt
                .Set<PayOsCancellationRequest>()
                .SingleAsync();
            request.MarkProcessing(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-1), 1);
            await crashedAttempt.SaveChangesAsync();
        }

        var client = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        client
            .Setup(x => x.GetPaymentLinkInformationAsync(1001))
            .ReturnsAsync(Link("CANCELLED"));
        Assert.True(await fixture.DispatchAsync(client.Object));

        await using var db = fixture.Context();
        PayOsCancellationRequest completed = await db.Set<PayOsCancellationRequest>().SingleAsync();
        Assert.Equal(PayOsCancellationState.Completed, completed.State);
        Assert.Equal(2, completed.Attempts);
        Assert.Equal(OrderStatus.Cancelled, (await db.Set<Order>().SingleAsync()).Status);
        client.Verify(
            x => x.CancelPaymentLinkAsync(It.IsAny<long>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ConcurrentWorkersCannotProcessTheSameRequest()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<PaymentLinkInformation>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var firstClient = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        firstClient.Setup(x => x.GetPaymentLinkInformationAsync(1001)).Returns(() =>
        {
            entered.TrySetResult();
            return release.Task;
        });

        Task<bool> first = fixture.DispatchAsync(firstClient.Object);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var secondClient = new Mock<IOrderPaymentLinkClient>(MockBehavior.Strict);
        Assert.False(await fixture.DispatchAsync(secondClient.Object));
        secondClient.VerifyNoOtherCalls();

        release.SetResult(Link("CANCELLED"));
        Assert.True(await first);
        firstClient.Verify(x => x.GetPaymentLinkInformationAsync(1001), Times.Once);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task NonPayOsCancellationStillCompletesSynchronously()
    {
        await using var fixture = await Fixture.CreateAsync(OrderStatus.Pending);

        Assert.True((await fixture.RequestCancellationAsync()).IsSuccess);

        await using var db = fixture.Context();
        Assert.Equal(OrderStatus.Cancelled, (await db.Set<Order>().SingleAsync()).Status);
        Assert.Empty(await db.Set<PayOsCancellationRequest>().ToListAsync());
    }

    private static PaymentLinkInformation Link(string status) =>
        new(
            id: "payment-link",
            orderCode: 1001,
            amount: 100,
            amountPaid: status == "PAID" ? 100 : 0,
            amountRemaining: status == "PAID" ? 0 : 100,
            status: status,
            createdAt: "2026-09-13T00:00:00Z",
            transactions: [],
            canceledAt: status == "CANCELLED" ? "2026-09-13T01:00:00Z" : null,
            cancellationReason: status == "CANCELLED" ? "Customer request" : null
        );

    private sealed class Fixture(NpgsqlDataSource source) : IAsyncDisposable
    {
        public TheDbContext Context() => new(
            new DbContextOptionsBuilder<TheDbContext>()
                .EnableServiceProviderCaching(false)
                .UseNpgsql(source)
                .Options
        );

        public static async Task<Fixture> CreateAsync(
            OrderStatus status = OrderStatus.Processed
        )
        {
            string connection = Environment.GetEnvironmentVariable(
                "VIETWASH_SEED_TEST_DATABASE"
            )!;
            var builder = new NpgsqlConnectionStringBuilder(connection);
            Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
            Assert.StartsWith("vietwash_seed_test", builder.Database);
            string schema = "payos_cancel_" + Guid.NewGuid().ToString("N");
            await using var admin = new NpgsqlConnection(connection);
            await admin.OpenAsync();
            await new NpgsqlCommand(
                $"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}",
                admin
            ).ExecuteNonQueryAsync();
            builder.SearchPath = $"{schema},public";
            var fixture = new Fixture(
                new NpgsqlDataSourceBuilder(builder.ConnectionString).EnableDynamicJson().Build()
            );
            await using var db = fixture.Context();
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            db.Set<User>().Add(
                new User("Saga staff", "staff@example.test", "0900000001", "STAFF", "STAFF-7")
                {
                    Id = 7,
                    Status = ActivationStatus.Active,
                }
            );
            db.Set<Order>().Add(
                new Order(2, 7, "OD-1001", 100, 100, status, customerId: null)
                {
                    Id = 1001,
                }
            );
            await db.SaveChangesAsync();
            return fixture;
        }

        public async Task<Result> RequestCancellationAsync()
        {
            var db = Context();
            using var unitOfWork = new UnitOfWork(
                db,
                Log.Logger,
                Mock.Of<IMemoryCacheService>()
            );
            return await new UpdateStatusHandler(unitOfWork, Actor()).Handle(
                new UpdateStatusCommand
                {
                    OrderId = "1001",
                    Model = new OrderUpdateStatus
                    {
                        Status = OrderStatus.Cancelled,
                        CancellationReason = "Customer request",
                    },
                },
                CancellationToken.None
            );
        }

        public async Task<bool> DispatchAsync(IOrderPaymentLinkClient client)
        {
            await using var db = Context();
            return await new PayOsCancellationDispatcher(db, client, Log.Logger)
                .DispatchOneAsync(CancellationToken.None);
        }

        public async Task MakeDueAsync()
        {
            await using var db = Context();
            _ = await db.Set<PayOsCancellationRequest>().ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    x => x.NextAttemptAt,
                    DateTimeOffset.UtcNow.AddMinutes(-1)
                )
            );
        }

        public ValueTask DisposeAsync() => source.DisposeAsync();

        private static ICurrentAccount Actor()
        {
            var actor = new Mock<ICurrentAccount>();
            actor.SetupGet(x => x.Id).Returns(7);
            actor.SetupGet(x => x.Session).Returns(
                new UserAuth { Id = 7, Role = "STAFF", Branches = ["2"] }
            );
            return actor.Object;
        }
    }
}
