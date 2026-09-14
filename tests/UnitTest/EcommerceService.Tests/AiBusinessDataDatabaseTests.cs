using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.AiAssistant.Models;
using Application.Feature.AiAssistant.Tools;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Products;
using Domain.Aggregates.Services;
using Domain.Aggregates.Users;
using Infrastructure.Data;
using Infrastructure.UnitOfWorks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Npgsql;
using Serilog;

namespace EcommerceService.Tests;

public class AiBusinessDataDatabaseTests
{
    [DevelopmentSeedDatabaseFact]
    public async Task ReadOnlyTools_QueryRealPostgresDataWithoutMutatingIt()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        await using TheDbContext context = database.CreateContext();
        using IUnitOfWork unitOfWork = new UnitOfWork(
            context,
            new LoggerConfiguration().CreateLogger(),
            Mock.Of<IMemoryCacheService>()
        );
        var service = new AiBusinessDataService(
            unitOfWork,
            Mock.Of<ISender>(MockBehavior.Strict)
        );
        var from = new DateOnly(2026, 9, 1);
        var to = new DateOnly(2026, 9, 7);

        OrderStatisticsResult orders = await service.GetOrderStatisticsAsync(
            new GetOrderStatisticsInput(1, from, to),
            TimeZoneInfo.Utc,
            CancellationToken.None
        );
        InventoryAlertsResult inventory = await service.GetInventoryAlertsAsync(
            new GetInventoryAlertsInput(1, 2),
            CancellationToken.None
        );
        EquipmentUtilizationResult equipment = await service.GetEquipmentUtilizationAsync(
            new GetEquipmentUtilizationInput(1, from, to),
            TimeZoneInfo.Utc,
            CancellationToken.None
        );

        Assert.Equal(1, orders.TotalOrders);
        Assert.Equal(1, Assert.Single(orders.ByStatus).Count);
        InventoryAlertItem alert = Assert.Single(inventory.Items);
        Assert.Equal("Nước giặt", alert.Name);
        Assert.Equal(1, alert.CurrentStock);
        Assert.Equal(1, equipment.TotalEquipment);
        Assert.Equal(1, equipment.ActiveEquipment);
        Assert.Equal(1, equipment.CurrentlyInUse);
        Assert.Equal(1, equipment.DistinctEquipmentUsed);
        Assert.Equal(1, equipment.AssignmentCount);
        Assert.Equal(100m, equipment.UtilizationPercent);
        Assert.Empty(context.ChangeTracker.Entries().Where(entry => entry.State != EntityState.Unchanged));
    }

    private sealed class TestDatabase(
        string adminConnection,
        string schema,
        NpgsqlDataSource dataSource
    ) : IAsyncDisposable
    {
        public static async Task<TestDatabase> CreateAsync()
        {
            string connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
            var builder = new NpgsqlConnectionStringBuilder(connection);
            Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
            Assert.StartsWith("vietwash_seed_test", builder.Database);
            string schema = "ai_tools_" + Guid.NewGuid().ToString("N");
            await using (var admin = new NpgsqlConnection(connection))
            {
                await admin.OpenAsync();
                await new NpgsqlCommand(
                    $"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}",
                    admin
                ).ExecuteNonQueryAsync();
            }

            builder.SearchPath = $"{schema},public";
            NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(builder.ConnectionString)
                .EnableDynamicJson()
                .Build();
            var database = new TestDatabase(connection, schema, dataSource);
            await using TheDbContext context = database.CreateContext();
            await context.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            await SeedAsync(context);
            return database;
        }

        public TheDbContext CreateContext() =>
            new(
                new DbContextOptionsBuilder<TheDbContext>()
                    .EnableServiceProviderCaching(false)
                    .UseNpgsql(dataSource)
                    .Options
            );

        public async ValueTask DisposeAsync()
        {
            await dataSource.DisposeAsync();
            await using var admin = new NpgsqlConnection(adminConnection);
            await admin.OpenAsync();
            await new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE", admin)
                .ExecuteNonQueryAsync();
        }

        private static async Task SeedAsync(TheDbContext context)
        {
            DateTimeOffset createdAt = new(2026, 9, 3, 8, 0, 0, TimeSpan.Zero);
            var staff = new User(
                "AI test staff",
                "ai-staff@example.test",
                "0900000010",
                "STAFF",
                "AI-STAFF"
            )
            {
                Id = 7,
                Status = ActivationStatus.Active,
            };
            var category = new Category("Hóa phẩm", null, ActivationStatus.Active, "CHEM")
            {
                Id = 10,
            };
            var product = new BranchProduct(
                1,
                "Nước giặt",
                "DETERGENT",
                ActivationStatus.Active,
                10,
                category.Id
            )
            {
                Id = 20,
                Category = category,
            };
            var unit = new UnitRelation
            {
                Id = 30,
                BranchProductId = product.Id,
                BranchProduct = product,
                Name = "Lít",
                BaseUnit = true,
                Multiple = 1,
                Status = ActivationStatus.Active,
            };
            product.UnitRelations.Add(unit);
            var import = new InventoryDocument("AI-IMPORT", 50, InventoryType.Import, 1)
            {
                Id = 40,
                Status = InventoryStatus.Completed,
                TransactionAt = createdAt,
            };
            var export = new InventoryDocument("AI-EXPORT", 40, InventoryType.Export, 1)
            {
                Id = 41,
                Status = InventoryStatus.Completed,
                TransactionAt = createdAt,
            };
            import.ProductSupplyings.Add(
                new ProductSupplying
                {
                    Id = 50,
                    ProductId = product.Id,
                    Product = product,
                    UnitRelationId = unit.Id,
                    UnitRelation = unit,
                    Quantity = 5,
                }
            );
            export.ProductSupplyings.Add(
                new ProductSupplying
                {
                    Id = 51,
                    ProductId = product.Id,
                    Product = product,
                    UnitRelationId = unit.Id,
                    UnitRelation = unit,
                    Quantity = -4,
                }
            );
            var machine = new Equipment(1, "Máy giặt", "AI-WASHER", 100, EquipmentStatus.Active)
            {
                Id = 60,
                Using = true,
            };
            var order = new Order(
                1,
                staff.Id,
                "AI-ORDER",
                100,
                100,
                OrderStatus.Completed,
                orderDate: createdAt,
                orderEquipments:
                [
                    new OrderEquipment
                    {
                        Id = 70,
                        EquipmentId = machine.Id,
                        Equipment = machine,
                        EquipmentName = machine.Name,
                    },
                ]
            )
            {
                Id = 80,
                CreatedAt = createdAt,
            };

            context.AddRange(staff, category, product, import, export, machine, order);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }
    }
}
