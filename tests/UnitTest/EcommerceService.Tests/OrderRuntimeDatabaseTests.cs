using Application.Common.Auth;
using Application.Common.HandleEventDomains;
using Application.Common.HandleEventDomains.Orders;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.UpdateStatus;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.PubSubLogs;
using Domain.Aggregates.Users;
using Domain.Events;
using Grpc.Core;
using Infrastructure.Data;
using Infrastructure.Notifications;
using Infrastructure.Data.Interceptors;
using Infrastructure.Services.DistributedCache;
using Infrastructure.UnitOfWorks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Notification_Grpc;
using Npgsql;
using Serilog;
using StackExchange.Redis;
using Order = Domain.Aggregates.Orders.Order;

namespace EcommerceService.Tests;

public class OrderRuntimeDatabaseTests
{
    [DevelopmentSeedDatabaseFact]
    public async Task NotificationDown_ProcessedReleasesEquipment_ThenCashCommitsDespiteRedisFailure()
    {
        // Use the existing opt-in isolated database, but do not run or change the development seed.
        foreach (bool redisThrows in new[] { false, true })
            await VerifyRuntimeFlow(redisThrows);
    }

    private static async Task VerifyRuntimeFlow(bool redisThrows)
    {
        string connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
        var builder = new NpgsqlConnectionStringBuilder(connection);
        Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
        Assert.StartsWith("vietwash_seed_test", builder.Database);
        string schema = "runtime_" + Guid.NewGuid().ToString("N");
        await using (var admin = new NpgsqlConnection(connection))
        {
            await admin.OpenAsync();
            await new NpgsqlCommand($"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}", admin).ExecuteNonQueryAsync();
        }
        builder.SearchPath = $"{schema},public";
        await using var dataSource = new NpgsqlDataSourceBuilder(builder.ConnectionString).EnableDynamicJson().Build();
        var logs = new NotificationLogSink();
        using var logger = new LoggerConfiguration().WriteTo.Sink(logs).CreateLogger();
        var notificationFailure = new RpcException(new Status(StatusCode.Unavailable, "Notification service DOWN"));
        var notification = new Mock<INotificationGrpc>(MockBehavior.Strict);
        notification.Setup(x => x.SendNotifyAsync(It.IsAny<SendNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(notificationFailure);

        // Exercise the actual Redis PubSubService handling (zero subscribers AND thrown transport errors).
        var subscriber = new Mock<ISubscriber>(MockBehavior.Strict);
        var publish = subscriber.Setup(x => x.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()));
        if (redisThrows)
            publish.ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Test Redis unavailable"));
        else
            publish.ReturnsAsync(0L);
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(x => x.GetSubscriber(It.IsAny<object>())).Returns(subscriber.Object);
        var bus = new PubSubService(redis.Object, Options.Create(new PubSubSettings { ChannelPrefix = "test" }), logger);
        var factory = new Mock<IPubSubFactory>(MockBehavior.Strict);
        factory.Setup(x => x.GetPubSub(PubSubType.Origin)).Returns(bus);
        var actor = Mock.Of<ICurrentAccount>(x => x.Id == 7 && x.Session == new UserAuth
        {
            Id = 7,
            Role = "STAFF",
            Branches = new[] { "2" },
        });
        var events = new List<INotification>();
        var services = new ServiceCollection()
            .AddSingleton<ILogger>(logger)
            .AddSingleton(actor)
            .AddSingleton(Mock.Of<IMemoryCacheService>())
            .AddSingleton(notification.Object)
            .AddSingleton(factory.Object)
            .AddScoped<EInvoiceEventHandler>()
            .AddScoped<CreateFundEventHandler>()
            .AddScoped<IPublisher>(sp =>
            {
                // Route only the known events to REAL handlers in the scope created by the real interceptor.
                var publisher = new Mock<IPublisher>(MockBehavior.Strict);
                publisher.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                    .Returns((INotification evt, CancellationToken token) =>
                    {
                        events.Add(evt);
                        return evt switch
                        {
                            UpdateStatusOrderEvent => ValueTask.CompletedTask, // captured by the same DbContext
                            EInvoiceEvent invoice => sp.GetRequiredService<EInvoiceEventHandler>().Handle(invoice, token),
                            CreateFundEvent fund => sp.GetRequiredService<CreateFundEventHandler>().Handle(fund, token),
                            _ => throw new InvalidOperationException($"Unexpected runtime event {evt.GetType().Name}"),
                        };
                    });
                return publisher.Object;
            })
            .AddSingleton<DispatchDomainEventInterceptor>()
            .AddSingleton<UpdateAuditableEntityInterceptor>()
            .AddDbContext<TheDbContext>((sp, options) => options.UseNpgsql(dataSource)
                .AddInterceptors(sp.GetRequiredService<UpdateAuditableEntityInterceptor>(), sp.GetRequiredService<DispatchDomainEventInterceptor>()))
            .AddScoped<IDbContext>(sp => sp.GetRequiredService<TheDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>();
        await using var provider = services.BuildServiceProvider();
        await using (var setup = provider.CreateAsyncScope())
        {
            var db = setup.ServiceProvider.GetRequiredService<TheDbContext>();
            await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            db.Set<User>().AddRange(
                new User("Runtime staff", "staff@example.test", "0900000001", "STAFF", "STAFF-7")
                {
                    Id = 7,
                    Status = ActivationStatus.Active,
                    BranchUsers = [new BranchUser { BranchId = 2, BranchName = "Branch Two" }],
                },
                new User("Runtime customer", "customer@example.test", "0900000002", "CUSTOMER", "CUS-501")
                { Id = 501, Status = ActivationStatus.Active });
            db.Set<Equipment>().Add(new Equipment(2, "Washer", "WM-21", 100, EquipmentStatus.Active) { Id = 21, Using = true });
            db.Set<Order>().Add(new Order(2, 7, "OD-1001", 100, 110, OrderStatus.InProgress, customerId: 501)
            {
                Id = 1001,
                OrderEquipments = [new OrderEquipment { EquipmentId = 21, EquipmentName = "Washer" }],
            });
            await db.SaveChangesAsync();
        }

        async Task Transition(OrderStatus target, PaymentMethod? payment = null)
        {
            await using var request = provider.CreateAsyncScope();
            var uow = request.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var result = await new UpdateStatusHandler(uow, actor).Handle(new UpdateStatusCommand
            {
                OrderId = "1001",
                Model = new OrderUpdateStatus { Status = target, PaymentMethod = payment },
            }, CancellationToken.None);
            Assert.True(result.IsSuccess, result.Error?.ToString());
        }

        // Rolling back the business transaction also discards its notification intent.
        await using (var rollback = provider.CreateAsyncScope())
        {
            var db = rollback.ServiceProvider.GetRequiredService<TheDbContext>();
            await using var tx = await db.Database.BeginTransactionAsync();
            var order = await db.Set<Order>().SingleAsync();
            order.TransitionTo(OrderStatus.Processed);
            await db.SaveChangesAsync();
            Assert.Single(await db.Set<NotificationOutbox>().ToListAsync());
            await tx.RollbackAsync();
        }
        events.Clear();
        await using (var checkRollback = provider.CreateAsyncScope())
        {
            var db = checkRollback.ServiceProvider.GetRequiredService<TheDbContext>();
            Assert.Empty(await db.Set<NotificationOutbox>().ToListAsync());
            Assert.Equal(OrderStatus.InProgress, (await db.Set<Order>().SingleAsync()).Status);
        }
        await Transition(OrderStatus.Processed);
        await using (var verification = provider.CreateAsyncScope())
        {
            var db = verification.ServiceProvider.GetRequiredService<TheDbContext>();
            Assert.Equal(OrderStatus.Processed, (await db.Set<Order>().SingleAsync()).Status);
            Assert.False((await db.Set<Equipment>().SingleAsync()).Using);
        }
        Assert.Single(events.OfType<UpdateStatusOrderEvent>());
        notification.VerifyNoOtherCalls(); // no network side effects before commit
        await using (var outboxCheck = provider.CreateAsyncScope())
            Assert.Single(await outboxCheck.ServiceProvider.GetRequiredService<TheDbContext>().Set<NotificationOutbox>().ToListAsync());

        await Transition(OrderStatus.Completed, PaymentMethod.Cash);
        await using (var verification = provider.CreateAsyncScope())
        {
            var db = verification.ServiceProvider.GetRequiredService<TheDbContext>();
            var order = await db.Set<Order>().SingleAsync();
            Assert.Equal(OrderStatus.Completed, order.Status);
            Assert.Equal(PaymentMethod.Cash, order.PaymentMethod);
            Assert.NotNull(order.OrderDate);
            Assert.False((await db.Set<Equipment>().SingleAsync()).Using);
        }
        Assert.Single(events.OfType<EInvoiceEvent>());
        Assert.Single(events.OfType<CreateFundEvent>());
        subscriber.Verify(x => x.PublishAsync(It.IsAny<RedisChannel>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Exactly(2));
        var count = events.Count;
        await Transition(OrderStatus.Completed, PaymentMethod.Cash);
        Assert.Equal(count, events.Count); // A retried Cash confirmation does not republish completion events.
    }
}
