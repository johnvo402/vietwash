using System.Text.Json;
using Application.Common.Auth;
using Application.Common.HandleEventDomains;
using Application.Common.HandleEventDomains.Orders;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.PubSubLogs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Domain.Events;
using Domain.Events.Enums;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using Infrastructure.IntegrationEvents;
using Infrastructure.UnitOfWorks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Npgsql;
using Serilog;

namespace EcommerceService.Tests;

public class IntegrationOutboxTests
{
    [Fact]
    public async Task OrderEInvoiceEvent_CannotUseDirectDomainEventPublisher()
    {
        var handler = new EInvoiceEventHandler();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await handler.Handle(
                    new EInvoiceEvent { Order = CompletedOrder() },
                    CancellationToken.None
                )
        );

        Assert.Contains("IntegrationOutbox", error.Message);
    }

    [Fact]
    public async Task OrderFinanceEvent_CannotUseDirectDomainEventPublisher()
    {
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        var handler = new CreateFundEventHandler(Log.Logger, factory.Object);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await handler.Handle(
                    new CreateFundEvent { FundEventType = FundEventType.Order },
                    CancellationToken.None
                )
        );

        Assert.Contains("IntegrationOutbox", error.Message);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void CompletedOrder_MapsExternalSideEffectsToStableOutboxMessages()
    {
        Order order = CompletedOrder();

        IReadOnlyList<IntegrationOutbox> messages = IntegrationOutbox.FromOrder(order);

        Assert.Equal(2, messages.Count);
        IntegrationOutbox fund = Assert.Single(
            messages,
            x => x.Id == "order-completed:1001:finance"
        );
        IntegrationOutbox invoice = Assert.Single(
            messages,
            x => x.Id == "order-completed:1001:einvoice"
        );
        Assert.NotEqual(
            Guid.Empty,
            JsonSerializer.Deserialize<CreateFundEvent>(fund.Payload)!.MessageId
        );
        Assert.NotEqual(
            Guid.Empty,
            JsonSerializer.Deserialize<EInvoiceOrderMessage>(invoice.Payload)!.MessageId
        );
    }

    [Fact]
    public async Task InternalDomainEvent_IsDequeuedOnlyAfterSuccessfulDispatch()
    {
        var publisher = new Mock<IPublisher>(MockBehavior.Strict);
        publisher
            .SetupSequence(x =>
                x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())
            )
            .Throws(new InvalidOperationException("first dispatch failed"))
            .Returns(ValueTask.CompletedTask);
        using ServiceProvider services = new ServiceCollection()
            .AddSingleton<IPublisher>(publisher.Object)
            .BuildServiceProvider();
        var dispatcher = new DispatchDomainEventInterceptor(
            services.GetRequiredService<IServiceScopeFactory>()
        );
        using var context = new TheDbContext(
            new DbContextOptionsBuilder<TheDbContext>()
                .UseNpgsql("Host=localhost;Database=model_only")
                .Options
        );
        var order = new Order(2, 7, "OD-1001", 100, 100, OrderStatus.InProgress);
        Assert.Equal(OrderTransitionResult.Applied, order.TransitionTo(OrderStatus.Processed));
        context.Attach(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchDomainEventsAsync(context)
        );
        Assert.IsType<UpdateStatusOrderEvent>(Assert.Single(order.UncommittedEvents));
        await dispatcher.DispatchDomainEventsAsync(context);
        Assert.Empty(order.UncommittedEvents);
        await dispatcher.DispatchDomainEventsAsync(context);
        publisher.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2)
        );
    }

    [DevelopmentSeedDatabaseFact]
    public async Task OuterTransactionRollback_DiscardsIntegrationEventsWithoutPublishing()
    {
        await using Fixture fixture = await Fixture.CreateAsync();
        await using AsyncServiceScope scope = fixture.Provider.CreateAsyncScope();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        TheDbContext db = scope.ServiceProvider.GetRequiredService<TheDbContext>();
        _ = await unitOfWork.BeginTransactionAsync();
        Order order = await db.Set<Order>().Include(x => x.OrderItems).SingleAsync();
        Assert.Equal(
            OrderTransitionResult.Applied,
            order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash)
        );

        await unitOfWork.SaveAsync();
        Assert.Equal(2, await db.Set<IntegrationOutbox>().CountAsync());
        Assert.Single(await db.Set<VoucherUsage>().ToListAsync());
        fixture.Publisher.VerifyNoOtherCalls();
        await unitOfWork.RollbackAsync();

        await using TheDbContext verification = fixture.Context();
        Assert.Empty(await verification.Set<IntegrationOutbox>().ToListAsync());
        Assert.Empty(await verification.Set<VoucherUsage>().ToListAsync());
        Assert.Equal(OrderStatus.Processed, (await verification.Set<Order>().SingleAsync()).Status);
        fixture.Publisher.VerifyNoOtherCalls();

        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        Assert.False(await fixture.DispatchAsync(factory.Object));
        factory.VerifyNoOtherCalls();
    }

    [DevelopmentSeedDatabaseFact]
    public async Task FailedFinancePublish_RemainsPendingAndRetriesWithSameMessageId()
    {
        await using Fixture fixture = await Fixture.CreateAsync();
        await fixture.CompleteOrderAsync();
        await fixture.IgnoreEInvoiceAsync();

        var publishedIds = new List<Guid>();
        var transport = new Mock<IPubSubService>(MockBehavior.Strict);
        transport
            .Setup(x =>
                x.PublishAsync(It.IsAny<CreateFundEvent>(), IntegrationOutbox.CreateFundTopic)
            )
            .Returns((CreateFundEvent message, string _) =>
            {
                publishedIds.Add(message.MessageId);
                return Task.FromResult(publishedIds.Count > 1);
            });
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        factory.Setup(x => x.GetPubSub(PubSubType.Origin)).Returns(transport.Object);

        Assert.True(await fixture.DispatchAsync(factory.Object));
        await using (TheDbContext failedVerification = fixture.Context())
        {
            IntegrationOutbox pending = await failedVerification
                .Set<IntegrationOutbox>()
                .SingleAsync(x => x.Topic == IntegrationOutbox.CreateFundTopic);
            Assert.Null(pending.DeliveredAt);
            Assert.Null(pending.LockedUntil);
            Assert.Equal(1, pending.Attempts);
            Assert.True(pending.NextAttemptAt > pending.CreatedAt);
            Assert.Equal(nameof(InvalidOperationException), pending.LastError);
        }

        Assert.False(await fixture.DispatchAsync(factory.Object));
        await fixture.MakeFinanceDueAsync();
        Assert.True(await fixture.DispatchAsync(factory.Object));

        await using (TheDbContext deliveredVerification = fixture.Context())
        {
            IntegrationOutbox delivered = await deliveredVerification
                .Set<IntegrationOutbox>()
                .SingleAsync(x => x.Topic == IntegrationOutbox.CreateFundTopic);
            Assert.NotNull(delivered.DeliveredAt);
            Assert.Null(delivered.LastError);
            Assert.Equal(2, delivered.Attempts);
        }
        Assert.Equal(2, publishedIds.Count);
        Assert.NotEqual(Guid.Empty, publishedIds[0]);
        Assert.Equal(publishedIds[0], publishedIds[1]);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ConcurrentFinanceDispatchers_CannotPublishTheSameMessageTogether()
    {
        await using Fixture fixture = await Fixture.CreateAsync();
        await fixture.CompleteOrderAsync();
        await fixture.IgnoreEInvoiceAsync();

        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new Mock<IPubSubService>(MockBehavior.Strict);
        transport
            .Setup(x =>
                x.PublishAsync(It.IsAny<CreateFundEvent>(), IntegrationOutbox.CreateFundTopic)
            )
            .Returns((CreateFundEvent _, string _) =>
            {
                entered.SetResult();
                return release.Task;
            });
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        factory.Setup(x => x.GetPubSub(PubSubType.Origin)).Returns(transport.Object);

        Task<bool> first = fixture.DispatchAsync(factory.Object);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(await fixture.DispatchAsync(factory.Object));
        release.SetResult(true);
        Assert.True(await first);

        transport.Verify(
            x =>
                x.PublishAsync(It.IsAny<CreateFundEvent>(), IntegrationOutbox.CreateFundTopic),
            Times.Once
        );
        transport.VerifyNoOtherCalls();
    }

    [DevelopmentSeedDatabaseFact]
    public async Task FailedEInvoicePublish_RemainsPendingAndRetriesWithSameMessageId()
    {
        await using Fixture fixture = await Fixture.CreateAsync();
        await fixture.CompleteOrderAsync();
        await fixture.IgnoreFinanceAsync();

        var published = new List<EInvoiceOrderMessage>();
        var transport = new Mock<IPubSubService>(MockBehavior.Strict);
        transport
            .Setup(x =>
                x.PublishAsync(
                    It.IsAny<EInvoiceOrderMessage>(),
                    IntegrationOutbox.EInvoiceTopic
                )
            )
            .Returns((EInvoiceOrderMessage message, string _) =>
            {
                published.Add(message);
                return Task.FromResult(published.Count > 1);
            });
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        factory.Setup(x => x.GetPubSub(PubSubType.Origin)).Returns(transport.Object);

        Assert.True(await fixture.DispatchAsync(factory.Object));
        await using (TheDbContext failedVerification = fixture.Context())
        {
            IntegrationOutbox pending = await failedVerification
                .Set<IntegrationOutbox>()
                .SingleAsync(x => x.Topic == IntegrationOutbox.EInvoiceTopic);
            Assert.Null(pending.DeliveredAt);
            Assert.Null(pending.LockedUntil);
            Assert.Equal(1, pending.Attempts);
            Assert.True(pending.NextAttemptAt > pending.CreatedAt);
            Assert.Equal(nameof(InvalidOperationException), pending.LastError);
        }

        Assert.False(await fixture.DispatchAsync(factory.Object));
        await fixture.MakeEInvoiceDueAsync();
        Assert.True(await fixture.DispatchAsync(factory.Object));

        await using (TheDbContext deliveredVerification = fixture.Context())
        {
            IntegrationOutbox delivered = await deliveredVerification
                .Set<IntegrationOutbox>()
                .SingleAsync(x => x.Topic == IntegrationOutbox.EInvoiceTopic);
            Assert.NotNull(delivered.DeliveredAt);
            Assert.Null(delivered.LastError);
            Assert.Equal(2, delivered.Attempts);
        }
        Assert.Equal(2, published.Count);
        Assert.NotEqual(Guid.Empty, published[0].MessageId);
        Assert.Equal(published[0].MessageId, published[1].MessageId);
        Assert.Equal(published[0].OrderId, published[1].OrderId);
        Assert.Equal(published[0].OrderCode, published[1].OrderCode);
        Assert.Equal(published[0].Items.Count, published[1].Items.Count);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task ConcurrentEInvoiceDispatchers_CannotPublishTheSameMessageTogether()
    {
        await using Fixture fixture = await Fixture.CreateAsync();
        await fixture.CompleteOrderAsync();
        await fixture.IgnoreFinanceAsync();

        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var transport = new Mock<IPubSubService>(MockBehavior.Strict);
        transport
            .Setup(x =>
                x.PublishAsync(
                    It.IsAny<EInvoiceOrderMessage>(),
                    IntegrationOutbox.EInvoiceTopic
                )
            )
            .Returns((EInvoiceOrderMessage _, string _) =>
            {
                entered.SetResult();
                return release.Task;
            });
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        factory.Setup(x => x.GetPubSub(PubSubType.Origin)).Returns(transport.Object);

        Task<bool> first = fixture.DispatchAsync(factory.Object);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.False(await fixture.DispatchAsync(factory.Object));
        release.SetResult(true);
        Assert.True(await first);

        transport.Verify(
            x =>
                x.PublishAsync(
                    It.IsAny<EInvoiceOrderMessage>(),
                    IntegrationOutbox.EInvoiceTopic
                ),
            Times.Once
        );
        transport.VerifyNoOtherCalls();
    }

    [DevelopmentSeedDatabaseFact]
    public async Task CommitPersistsIntegrationEvents_AndDeliveredRowsAreNotPublishedAgain()
    {
        await using Fixture fixture = await Fixture.CreateAsync();
        await using (AsyncServiceScope scope = fixture.Provider.CreateAsyncScope())
        {
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            TheDbContext db = scope.ServiceProvider.GetRequiredService<TheDbContext>();
            _ = await unitOfWork.BeginTransactionAsync();
            Order order = await db.Set<Order>().Include(x => x.OrderItems).SingleAsync();
            Assert.Equal(
                OrderTransitionResult.Applied,
                order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash)
            );

            await unitOfWork.SaveAsync();
            fixture.Publisher.VerifyNoOtherCalls();
            await unitOfWork.CommitAsync();
            fixture.Publisher.VerifyNoOtherCalls();
        }

        await using (TheDbContext verification = fixture.Context())
        {
            Assert.Equal(2, await verification.Set<IntegrationOutbox>().CountAsync());
            Assert.Single(await verification.Set<VoucherUsage>().ToListAsync());
            Assert.All(
                await verification.Set<IntegrationOutbox>().ToListAsync(),
                message => Assert.Null(message.DeliveredAt)
            );
        }

        await using (AsyncServiceScope retryScope = fixture.Provider.CreateAsyncScope())
        {
            IUnitOfWork retryUnitOfWork = retryScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            TheDbContext retryDb = retryScope.ServiceProvider.GetRequiredService<TheDbContext>();
            _ = await retryUnitOfWork.BeginTransactionAsync();
            Order completed = await retryDb.Set<Order>().SingleAsync();
            Assert.Equal(
                OrderTransitionResult.Idempotent,
                completed.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash)
            );
            await retryUnitOfWork.SaveAsync();
            await retryUnitOfWork.CommitAsync();
        }

        await using (TheDbContext retryVerification = fixture.Context())
        {
            Assert.Equal(2, await retryVerification.Set<IntegrationOutbox>().CountAsync());
            Assert.Single(await retryVerification.Set<VoucherUsage>().ToListAsync());
        }

        var transport = new Mock<IPubSubService>(MockBehavior.Strict);
        transport
            .Setup(x =>
                x.PublishAsync(
                    It.Is<CreateFundEvent>(e =>
                        e.MessageId != Guid.Empty
                    ),
                    IntegrationOutbox.CreateFundTopic
                )
            )
            .ReturnsAsync(true);
        transport
            .Setup(x =>
                x.PublishAsync(
                    It.Is<EInvoiceOrderMessage>(e =>
                        e.MessageId != Guid.Empty
                    ),
                    IntegrationOutbox.EInvoiceTopic
                )
            )
            .ReturnsAsync(true);
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        factory.Setup(x => x.GetPubSub(PubSubType.Origin)).Returns(transport.Object);

        Assert.True(await fixture.DispatchAsync(factory.Object));
        Assert.True(await fixture.DispatchAsync(factory.Object));
        Assert.False(await fixture.DispatchAsync(factory.Object));
        transport.Verify(
            x =>
                x.PublishAsync(
                    It.Is<CreateFundEvent>(e =>
                        e.MessageId != Guid.Empty
                    ),
                    IntegrationOutbox.CreateFundTopic
                ),
            Times.Once
        );
        transport.Verify(
            x =>
                x.PublishAsync(
                    It.Is<EInvoiceOrderMessage>(e =>
                        e.MessageId != Guid.Empty
                    ),
                    IntegrationOutbox.EInvoiceTopic
                ),
            Times.Once
        );
        transport.VerifyNoOtherCalls();
    }

    private static Order CompletedOrder()
    {
        var order = new Order(2, 7, "OD-1001", 100, 100, OrderStatus.Processed)
        {
            Id = 1001,
        };
        _ = order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash);
        return order;
    }

    private sealed class Fixture(NpgsqlDataSource source, ServiceProvider provider)
        : IAsyncDisposable
    {
        public ServiceProvider Provider => provider;
        public Mock<IPublisher> Publisher { get; } = provider
            .GetRequiredService<Mock<IPublisher>>();

        public TheDbContext Context() =>
            new(new DbContextOptionsBuilder<TheDbContext>().UseNpgsql(source).Options);

        public static async Task<Fixture> CreateAsync()
        {
            string connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
            var builder = new NpgsqlConnectionStringBuilder(connection);
            Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
            Assert.StartsWith("vietwash_seed_test", builder.Database);
            string schema = "integration_outbox_" + Guid.NewGuid().ToString("N");
            await using (var admin = new NpgsqlConnection(connection))
            {
                await admin.OpenAsync();
                await new NpgsqlCommand(
                    $"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}",
                    admin
                ).ExecuteNonQueryAsync();
            }

            builder.SearchPath = $"{schema},public";
            NpgsqlDataSource source = new NpgsqlDataSourceBuilder(builder.ConnectionString)
                .EnableDynamicJson()
                .Build();
            var publisher = new Mock<IPublisher>(MockBehavior.Strict);
            ICurrentAccount actor = Mock.Of<ICurrentAccount>(x =>
                x.Id == 7
                && x.Session
                    == new UserAuth
                    {
                        Id = 7,
                        Role = "STAFF",
                        Branches = new[] { "2" },
                    }
            );
            ServiceProvider provider = new ServiceCollection()
                .AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger())
                .AddSingleton(Mock.Of<IMemoryCacheService>())
                .AddSingleton(actor)
                .AddSingleton(publisher)
                .AddSingleton<IPublisher>(publisher.Object)
                .AddSingleton<DispatchDomainEventInterceptor>()
                .AddDbContext<TheDbContext>((sp, options) =>
                    options
                        .UseNpgsql(source)
                        .AddInterceptors(
                            sp.GetRequiredService<DispatchDomainEventInterceptor>()
                        )
                )
                .AddScoped<IDbContext>(sp => sp.GetRequiredService<TheDbContext>())
                .AddScoped<IUnitOfWork, UnitOfWork>()
                .BuildServiceProvider();
            var fixture = new Fixture(source, provider);
            await using TheDbContext db = fixture.Context();
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            db.Set<User>()
                .AddRange(
                    new User(
                        "Outbox staff",
                        "staff@example.test",
                        "0900000001",
                        "STAFF",
                        "STAFF-7"
                    )
                    {
                        Id = 7,
                        Status = ActivationStatus.Active,
                    },
                    new User(
                        "Outbox customer",
                        "customer@example.test",
                        "0900000002",
                        "CUSTOMER",
                        "CUSTOMER-501"
                    )
                    {
                        Id = 501,
                        Status = ActivationStatus.Active,
                    }
                );
            db.Set<Voucher>()
                .Add(
                    new Voucher
                    {
                        Id = 50,
                        Code = "OUTBOX",
                        Title = "Outbox voucher",
                        Barcode = "OUTBOX-50",
                        DiscountFixed = true,
                        DiscountValue = 10,
                        Status = ActivationStatus.Active,
                    }
                );
            db.Set<Order>()
                .Add(
                    new Order(
                        2,
                        7,
                        "OD-1001",
                        100,
                        90,
                        OrderStatus.Processed,
                        voucherId: 50,
                        customerId: 501,
                        discountFixed: true,
                        discountValue: 10
                    )
                    {
                        Id = 1001,
                    }
                );
            await db.SaveChangesAsync();
            return fixture;
        }

        public async Task<bool> DispatchAsync(IPubSubFactory factory)
        {
            await using TheDbContext db = Context();
            return await new IntegrationOutboxDispatcher(db, factory, Log.Logger)
                .DispatchOneAsync(CancellationToken.None);
        }

        public async Task CompleteOrderAsync()
        {
            await using AsyncServiceScope scope = Provider.CreateAsyncScope();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            TheDbContext db = scope.ServiceProvider.GetRequiredService<TheDbContext>();
            _ = await unitOfWork.BeginTransactionAsync();
            Order order = await db.Set<Order>().Include(x => x.OrderItems).SingleAsync();
            Assert.Equal(
                OrderTransitionResult.Applied,
                order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash)
            );
            await unitOfWork.SaveAsync();
            await unitOfWork.CommitAsync();
        }

        public async Task IgnoreEInvoiceAsync()
        {
            await using TheDbContext db = Context();
            await db
                .Set<IntegrationOutbox>()
                .Where(x => x.Topic == IntegrationOutbox.EInvoiceTopic)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.DeliveredAt, DateTimeOffset.UtcNow)
                );
        }

        public async Task IgnoreFinanceAsync()
        {
            await using TheDbContext db = Context();
            await db
                .Set<IntegrationOutbox>()
                .Where(x => x.Topic == IntegrationOutbox.CreateFundTopic)
                .ExecuteUpdateAsync(setters =>
                    setters.SetProperty(x => x.DeliveredAt, DateTimeOffset.UtcNow)
                );
        }

        public async Task MakeFinanceDueAsync()
        {
            await using TheDbContext db = Context();
            await db
                .Set<IntegrationOutbox>()
                .Where(x => x.Topic == IntegrationOutbox.CreateFundTopic)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(x => x.NextAttemptAt, DateTimeOffset.UtcNow.AddMinutes(-1))
                        .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null)
                );
        }

        public async Task MakeEInvoiceDueAsync()
        {
            await using TheDbContext db = Context();
            await db
                .Set<IntegrationOutbox>()
                .Where(x => x.Topic == IntegrationOutbox.EInvoiceTopic)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(x => x.NextAttemptAt, DateTimeOffset.UtcNow.AddMinutes(-1))
                        .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null)
                );
        }

        public async ValueTask DisposeAsync()
        {
            await provider.DisposeAsync();
            await source.DisposeAsync();
        }
    }
}
