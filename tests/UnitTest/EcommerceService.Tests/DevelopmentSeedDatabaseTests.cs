using Application.Common.Auth;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Orders.Command.UpdateStatus;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Infrastructure.Data;
using Infrastructure.Data.Interceptors;
using Infrastructure.UnitOfWorks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Moq;
using Npgsql;
using Serilog;

namespace EcommerceService.Tests;

public sealed class DevelopmentSeedDatabaseFactAttribute : FactAttribute
{
    public DevelopmentSeedDatabaseFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")))
            Skip = "Set VIETWASH_SEED_TEST_DATABASE to an isolated local PostgreSQL test database.";
    }
}

public class DevelopmentSeedDatabaseTests
{
    [Fact]
    public async Task ProductionInitializer_DoesNotResolveDatabaseOrSeedDependencies()
    {
        using var provider = new ServiceCollection()
            .AddSingleton<IHostEnvironment>(new TestEnvironment { EnvironmentName = Environments.Production })
            .BuildServiceProvider();
        await DbInitializer.InitializeAsync(provider);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task FreshDatabase_WithAuthProjections_SeedsRepairsAndRunsCashDemoWithoutExternalServices()
    {
        string connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
        var builder = new NpgsqlConnectionStringBuilder(connection);
        Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
        Assert.StartsWith("vietwash_seed_test", builder.Database);
        // Each execution owns a fresh schema, never drops/recreates the caller's database.
        string schema = "seed_" + Guid.NewGuid().ToString("N");
        await using (var admin = new NpgsqlConnection(connection))
        {
            await admin.OpenAsync();
            await new NpgsqlCommand($"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}", admin).ExecuteNonQueryAsync();
        }
        builder.SearchPath = $"{schema},public";
        await using var dataSource = new NpgsqlDataSourceBuilder(builder.ConnectionString).EnableDynamicJson().Build();
        var publisher = new Mock<IPublisher>(MockBehavior.Strict);
        var encryption = new Mock<IEncryptionService>();
        encryption.Setup(x => x.Encrypt(It.IsAny<string>())).Returns<string>(x => x);
        var qr = new Mock<IQrGenerator>();
        qr.Setup(x => x.GenerateQrBase64(It.IsAny<string>())).Returns("test-qr");
        var actor = Mock.Of<ICurrentAccount>(x => x.Id == 7 && x.Session == new UserAuth
        {
            Id = 7,
            Role = "STAFF",
            Branches = new[] { "1", "2", "3" },
        });
        var services = new ServiceCollection()
            .AddSingleton<IHostEnvironment>(new TestEnvironment())
            .AddSingleton<ILogger>(new LoggerConfiguration().CreateLogger())
            .AddSingleton(Mock.Of<IMemoryCacheService>())
            .AddSingleton(actor)
            .AddSingleton(encryption.Object).AddSingleton(qr.Object)
            .AddSingleton(publisher.Object)
            .AddSingleton<DispatchDomainEventInterceptor>()
            .AddSingleton<UpdateAuditableEntityInterceptor>()
            .AddDbContext<TheDbContext>((sp, options) => options.UseNpgsql(dataSource)
                .AddInterceptors(sp.GetRequiredService<UpdateAuditableEntityInterceptor>(), sp.GetRequiredService<DispatchDomainEventInterceptor>()))
            .AddScoped<IDbContext>(sp => sp.GetRequiredService<TheDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TheDbContext>();
        // The new schema is empty even when other test runs used this database.
        await db.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();

        async Task RunSeed()
        {
            // Each application startup has its own scoped DbContext/UnitOfWork.
            await using var seedScope = provider.CreateAsyncScope();
            await DbInitializer.InitializeAsync(seedScope.ServiceProvider);
            db.ChangeTracker.Clear();
        }

        var missingIdentity = await Assert.ThrowsAsync<InvalidOperationException>(RunSeed);
        Assert.Contains("Auth synchronization", missingIdentity.Message);
        Assert.Equal((0, 0, 0, 0), await Counts(db));
        publisher.VerifyNoOtherCalls();

        // Ecommerce stores projections, not login credentials. Simulate Auth/Project's existing handoff.
        db.Set<User>().AddRange(
            new User("Seed admin", "seed-admin@example.test", "0900000001", "ADMIN", "SEED-ADMIN") { Id = 1, Status = ActivationStatus.Active },
            new User("Seed customer", "seed-customer@example.test", "0900000002", "CUSTOMER", "SEED-CUSTOMER") { Id = 501, Status = ActivationStatus.Active },
            new User("Seed staff", "seed-staff@example.test", "0900000003", "STAFF", "SEED-STAFF")
            {
                Id = 7,
                Status = ActivationStatus.Active,
                BranchUsers = DevelopmentSeedPolicy.BranchIds.Select(id => new BranchUser { BranchId = id, BranchName = $"Branch {id}" }).ToList(),
            });
        await db.SaveChangesAsync();
        await RunSeed();
        publisher.VerifyNoOtherCalls(); // No Finance, Notification, invoice, voucher or completion event during seed.
        db.ChangeTracker.Clear();

        var orders = await db.Set<Order>().Include(x => x.OrderEquipments).ToListAsync();
        var equipments = await db.Set<Equipment>().ToListAsync();
        Assert.Equal(21, orders.Count);
        Assert.Equal(51, equipments.Count);
        DevelopmentSeedPolicy.ValidateOrders(orders, equipments);
        var claims = orders.Where(x => x.Status == OrderStatus.InProgress).SelectMany(x => x.OrderEquipments).Select(x => x.EquipmentId).ToArray();
        Assert.Equal(claims.Length, claims.Distinct().Count());
        Assert.Equal(claims.Order(), equipments.Where(x => x.Using).Select(x => x.Id).Order());
        foreach (long branchId in DevelopmentSeedPolicy.BranchIds)
        {
            Assert.Contains(equipments, x => x.BranchId == branchId && x.Status == EquipmentStatus.Active && !x.Using);
            Assert.True(await db.Set<InventoryDocument>().AnyAsync(x => x.BranchId == branchId && x.Type == InventoryType.Import && x.Status == InventoryStatus.Completed));
            Assert.False(await db.Set<ServiceTariff>().AnyAsync(x => x.Tariff.BranchId == branchId && x.Service.BranchId != branchId));
            var products = await db.Set<BranchProduct>().Where(x => x.BranchId == branchId).ToListAsync();
            var available = await Stock(db, products.Select(x => x.Id).ToArray());
            Assert.Equal(4, available.Count);
            Assert.All(available.Values, quantity => Assert.True(quantity > 0));
            var resources = await db.Set<ServiceResource>()
                .Where(x => x.UnitRelation.Service!.BranchId == branchId)
                .Include(x => x.BranchProduct).Include(x => x.UnitProduct).ToListAsync();
            Assert.NotEmpty(resources);
            Assert.All(resources, resource =>
            {
                Assert.Equal(branchId, resource.BranchProduct.BranchId);
                Assert.Equal(ActivationStatus.Active, resource.BranchProduct.Status);
                Assert.False(resource.BranchProduct.Disable);
                Assert.Equal(resource.ProductId, resource.UnitProduct.BranchProductId);
                Assert.Equal(ActivationStatus.Active, resource.UnitProduct.Status);
                Assert.True(resource.UnitProduct.Multiple > 0);
                Assert.True(resource.Quantity > 0);
                Assert.True(available[resource.ProductId] >= resource.Quantity * resource.UnitProduct.Multiple * 5);
            });
        }

        var seedExports = await db.Set<InventoryDocument>()
            .Where(x => x.Type == InventoryType.Export).Include(x => x.ProductSupplyings).ToListAsync();
        Assert.NotEmpty(seedExports);
        Assert.All(seedExports, export =>
        {
            Assert.Equal(InventoryStatus.Completed, export.Status);
            var source = Assert.Single(orders, order => order.Id == export.SourceOrderId);
            Assert.Equal(source.BranchId, export.BranchId);
            Assert.All(export.ProductSupplyings, line => Assert.True(line.Quantity < 0));
        });

        var before = await Counts(db);
        await RunSeed();
        Assert.Equal(before, await Counts(db));
        publisher.VerifyNoOtherCalls();

        // Reproduce the old completed-receipt/missing-equipment state without deleting orders or stock.
        var missingMachine = equipments.First(x => !orders.SelectMany(o => o.OrderEquipments).Any(link => link.EquipmentId == x.Id));
        await db.Set<Equipment>().Where(x => x.Id == missingMachine.Id).ExecuteDeleteAsync();
        db.ChangeTracker.Clear();
        await RunSeed();
        Assert.Equal(before, await Counts(db));
        Assert.Single(await db.Set<Equipment>().Where(x => x.BranchId == missingMachine.BranchId && x.Code == missingMachine.Code).ToListAsync());
        await RunSeed();
        Assert.Equal(before, await Counts(db));
        publisher.VerifyNoOtherCalls();

        // Real handlers and SQL for the cash demo. Capture runtime events instead of contacting external consumers.
        publisher.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);
        db.ChangeTracker.Clear();
        const long demoBranch = 2;
        var service = await db.Set<Service>().Include(x => x.UnitRelations).ThenInclude(x => x.AsUnitRelation)
            .SingleAsync(x => x.BranchId == demoBranch && x.Name == "Combo Giặt Sấy Quần Áo");
        var unit = Assert.Single(service.UnitRelations);
        Assert.NotEmpty(unit.AsUnitRelation);
        var tariff = await db.Set<Tariff>().SingleAsync(x => x.BranchId == demoBranch && x.Name == "Bảng giá chung");
        var equipment = await db.Set<Equipment>().FirstAsync(x => x.BranchId == demoBranch && x.Status == EquipmentStatus.Active && !x.Using);
        var stockBefore = await Stock(db, unit.AsUnitRelation.Select(x => x.ProductId).ToArray());
        Assert.All(stockBefore.Values, amount => Assert.True(amount > 0));
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var created = await new CreateOrderHandler(uow, actor, new OrgSetting { VatPercent = 10 }, encryption.Object, qr.Object)
            .Handle(new CreateOrderCommand
            {
                BranchId = demoBranch,
                CustomerId = 501,
                TariffId = tariff.Id,
                OrderItems = [new() { ServiceId = service.Id, UnitRelationId = unit.Id, Quantity = 5 }],
            }, CancellationToken.None);
        Assert.True(created.IsSuccess, created.Error?.ToString());
        long orderId = created.Value!.Id;
        Assert.Equal(OrderStatus.Pending, (await db.Set<Order>().AsNoTracking().SingleAsync(x => x.Id == orderId)).Status);

        async Task Transition(OrderStatus status, PaymentMethod? payment = null, long? machine = null)
        {
            db.ChangeTracker.Clear();
            await using var requestScope = provider.CreateAsyncScope();
            var requestUnitOfWork = requestScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var result = await new UpdateStatusHandler(requestUnitOfWork, actor).Handle(new UpdateStatusCommand
            {
                OrderId = orderId.ToString(),
                Model = new OrderUpdateStatus
                {
                    Status = status,
                    PaymentMethod = payment,
                    OrderEquipments = machine.HasValue ? [new() { EquipmentId = machine.Value }] : [],
                },
            }, CancellationToken.None);
            Assert.True(result.IsSuccess, result.Error?.ToString());
        }
        await Transition(OrderStatus.InProgress, machine: equipment.Id);
        Assert.True((await db.Set<Equipment>().AsNoTracking().SingleAsync(x => x.Id == equipment.Id)).Using);
        var export = await db.Set<InventoryDocument>().Include(x => x.ProductSupplyings).SingleAsync(x => x.SourceOrderId == orderId);
        Assert.Equal(InventoryStatus.Completed, export.Status);
        Assert.Equal(InventoryType.Export, export.Type);
        Assert.Equal(demoBranch, export.BranchId);
        Assert.All(export.ProductSupplyings, x => Assert.True(x.Quantity < 0));
        var stockAfter = await Stock(db, stockBefore.Keys.ToArray());
        Assert.All(stockBefore.Keys, productId => Assert.True(stockAfter[productId] < stockBefore[productId]));
        Assert.Equal(stockBefore[unit.AsUnitRelation.First(x => x.Quantity == 0.1m).ProductId] - 0.5m,
            stockAfter[unit.AsUnitRelation.First(x => x.Quantity == 0.1m).ProductId]);
        Assert.Equal(stockBefore[unit.AsUnitRelation.First(x => x.Quantity == 0.05m).ProductId] - 0.75m,
            stockAfter[unit.AsUnitRelation.First(x => x.Quantity == 0.05m).ProductId]);
        await Transition(OrderStatus.Processed);
        Assert.False((await db.Set<Equipment>().AsNoTracking().SingleAsync(x => x.Id == equipment.Id)).Using);
        await Transition(OrderStatus.Completed, PaymentMethod.Cash);
        var completed = await db.Set<Order>().AsNoTracking().SingleAsync(x => x.Id == orderId);
        Assert.Equal(OrderStatus.Completed, completed.Status);
        Assert.Equal(PaymentMethod.Cash, completed.PaymentMethod);
        Assert.Single(await db.Set<InventoryDocument>().Where(x => x.SourceOrderId == orderId).ToListAsync());
        Assert.True(await db.Set<Order>().Where(x => x.Status == OrderStatus.Completed && x.BranchId == demoBranch).SumAsync(x => x.Total) > 0);
    }

    private static async Task<(int Orders, int Equipment, int Inventory, int Stock)> Counts(TheDbContext db) =>
        (await db.Set<Order>().CountAsync(), await db.Set<Equipment>().CountAsync(), await db.Set<InventoryDocument>().CountAsync(), await db.Set<ProductSupplying>().CountAsync());

    private static Task<Dictionary<long, decimal>> Stock(TheDbContext db, long[] productIds) =>
        db.Set<ProductSupplying>().Where(x => productIds.Contains(x.ProductId) && x.InventoryDocument.Status == InventoryStatus.Completed)
            .GroupBy(x => x.ProductId)
            .Select(x => new { ProductId = x.Key, Quantity = x.Sum(s => s.Quantity * s.UnitRelation.Multiple) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Quantity);

    private sealed class TestEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "SeedTests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
