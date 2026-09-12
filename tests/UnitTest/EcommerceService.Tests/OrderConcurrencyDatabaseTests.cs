using System.Data.Common;
using System.Linq.Expressions;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.Update;
using Application.Feature.Orders.Command.UpdateStatus;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Application.Common.Interfaces.Services.Cache;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Services;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Infrastructure.Data;
using Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Npgsql;
using Serilog;
using Shared.Kernel.Common.Specs.Interfaces;

namespace EcommerceService.Tests;

public class OrderConcurrencyDatabaseTests
{
    private const long OrderId = 1001;
    private const long TariffId = 100;
    private const long ServiceId = 20;
    private const long UnitRelationId = 30;
    private static readonly ICurrentAccount Actor = Mock.Of<ICurrentAccount>(x =>
        x.Id == 7
        && x.Session
            == new UserAuth
            {
                Id = 7,
                Role = "STAFF",
                Branches = new[] { "2" },
            }
    );

    [Fact]
    public void OrderVersion_IsConfiguredAsConcurrencyToken()
    {
        using var context = new TheDbContext(
            new DbContextOptionsBuilder<TheDbContext>()
                .UseNpgsql("Host=localhost;Database=model_only")
                .Options
        );

        Assert.True(
            context.Model.FindEntityType(typeof(Order))!.FindProperty(nameof(Order.Version))!
                .IsConcurrencyToken
        );
    }

    [DevelopmentSeedDatabaseFact]
    public async Task UpdateOrder_RacingUpdateStatus_ReturnsConflictWithoutRevertingStatus()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        var gate = new OrderReadGate();
        using var updateUnitOfWork = database.CreateUnitOfWork(gate);
        Task<Result> updateTask = new UpdateOrderHandler(updateUnitOfWork, Actor)
            .Handle(UpdateCommand("losing-details", 2), CancellationToken.None)
            .AsTask();

        await gate.WaitUntilReadAsync();
        Result statusResult;
        using (IUnitOfWork statusUnitOfWork = database.CreateUnitOfWork())
        {
            statusResult = await new UpdateStatusHandler(statusUnitOfWork, Actor).Handle(
                new UpdateStatusCommand
                {
                    OrderId = OrderId.ToString(),
                    Model = new OrderUpdateStatus
                    {
                        Status = OrderStatus.InProgress,
                        OrderEquipments = [new() { EquipmentId = 21 }],
                    },
                },
                CancellationToken.None
            );
        }

        gate.Release();
        Result updateResult = await updateTask;
        Assert.True(statusResult.IsSuccess, statusResult.Error?.Title);
        AssertConflict(updateResult);

        await using TheDbContext verification = database.CreateContext();
        Order persisted = await verification
            .Set<Order>()
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .SingleAsync(x => x.Id == OrderId);
        Assert.True(
            verification.Model.FindEntityType(typeof(Order))!.FindProperty(nameof(Order.Version))!
                .IsConcurrencyToken
        );
        Assert.Equal(OrderStatus.InProgress, persisted.Status);
        Assert.Equal("original", persisted.Note);
        Assert.Equal(1, Assert.Single(persisted.OrderItems).Quantity);
        Assert.Equal(1, persisted.Version);
    }

    [DevelopmentSeedDatabaseFact]
    public async Task TwoUpdateOrders_FromSameVersion_OnlyOneCommits()
    {
        await using TestDatabase database = await TestDatabase.CreateAsync();
        var firstGate = new OrderReadGate();
        var secondGate = new OrderReadGate();
        using var firstUnitOfWork = database.CreateUnitOfWork(firstGate);
        using var secondUnitOfWork = database.CreateUnitOfWork(secondGate);
        Task<Result> firstTask = new UpdateOrderHandler(firstUnitOfWork, Actor)
            .Handle(UpdateCommand("winner", 2), CancellationToken.None)
            .AsTask();
        Task<Result> secondTask = new UpdateOrderHandler(secondUnitOfWork, Actor)
            .Handle(UpdateCommand("loser", 3), CancellationToken.None)
            .AsTask();

        await Task.WhenAll(firstGate.WaitUntilReadAsync(), secondGate.WaitUntilReadAsync());
        firstGate.Release();
        Result firstResult = await firstTask;
        secondGate.Release();
        Result secondResult = await secondTask;

        Assert.True(firstResult.IsSuccess, firstResult.Error?.Title);
        AssertConflict(secondResult);
        await using TheDbContext verification = database.CreateContext();
        Order persisted = await verification
            .Set<Order>()
            .AsNoTracking()
            .Include(x => x.OrderItems)
            .SingleAsync(x => x.Id == OrderId);
        Assert.Equal(OrderStatus.Pending, persisted.Status);
        Assert.Equal("winner", persisted.Note);
        Assert.Equal(2, Assert.Single(persisted.OrderItems).Quantity);
        Assert.Equal(1, persisted.Version);
    }

    private static UpdateOrderCommand UpdateCommand(string note, int quantity) =>
        new()
        {
            OrderId = OrderId,
            Model = new UpdateOrderModel
            {
                TariffId = TariffId,
                Note = note,
                OrderItems =
                [
                    new()
                    {
                        ServiceId = ServiceId,
                        UnitRelationId = UnitRelationId,
                        Quantity = quantity,
                    },
                ],
            },
        };

    private static void AssertConflict(Result result)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(409, result.Error!.Status);
    }

    private sealed class TestDatabase(NpgsqlDataSource dataSource) : IAsyncDisposable
    {
        public static async Task<TestDatabase> CreateAsync()
        {
            string connection = Environment.GetEnvironmentVariable("VIETWASH_SEED_TEST_DATABASE")!;
            var builder = new NpgsqlConnectionStringBuilder(connection);
            Assert.Contains(builder.Host, new[] { "localhost", "127.0.0.1" });
            Assert.StartsWith("vietwash_seed_test", builder.Database);
            string schema = "concurrency_" + Guid.NewGuid().ToString("N");
            await using (var admin = new NpgsqlConnection(connection))
            {
                await admin.OpenAsync();
                await new NpgsqlCommand(
                    $"CREATE EXTENSION IF NOT EXISTS citext WITH SCHEMA public; CREATE SCHEMA {schema}",
                    admin
                ).ExecuteNonQueryAsync();
            }

            builder.SearchPath = $"{schema},public";
            var dataSource = new NpgsqlDataSourceBuilder(builder.ConnectionString)
                .EnableDynamicJson()
                .Build();
            var database = new TestDatabase(dataSource);
            await using TheDbContext context = database.CreateContext();
            await context.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();
            await SeedAsync(context);
            return database;
        }

        public TheDbContext CreateContext() =>
            new(
                new DbContextOptionsBuilder<TheDbContext>()
                    .UseNpgsql(dataSource)
                    .Options
            );

        public IUnitOfWork CreateUnitOfWork(OrderReadGate? gate = null)
        {
            IUnitOfWork inner = new UnitOfWork(
                CreateContext(),
                new LoggerConfiguration().CreateLogger(),
                Mock.Of<IMemoryCacheService>()
            );
            return gate is null ? inner : new BlockingOrderReadUnitOfWork(inner, gate);
        }

        public ValueTask DisposeAsync() => dataSource.DisposeAsync();

        private static async Task SeedAsync(TheDbContext context)
        {
            var staff = new User(
                "Concurrency staff",
                "staff@example.test",
                "0900000001",
                "STAFF",
                "STAFF-7"
            )
            {
                Id = 7,
                Status = ActivationStatus.Active,
            };
            var category = new Category("Laundry", null, ActivationStatus.Active, "LAUNDRY")
            {
                Id = 10,
            };
            var service = new Service(10, 2, "Wash", ActivationStatus.Active) { Id = ServiceId };
            var unit = new UnitRelation
            {
                Id = UnitRelationId,
                ServiceId = ServiceId,
                Name = "Kg",
                BaseUnit = true,
                Price = 90,
                Multiple = 1,
                ProcessingTime = 30,
                Status = ActivationStatus.Active,
            };
            var tariff = new Tariff("Standard", 2, ActivationStatus.Active) { Id = TariffId };
            var serviceTariff = new ServiceTariff
            {
                TariffId = TariffId,
                ServiceId = ServiceId,
                UnitRelationId = UnitRelationId,
                Price = 100,
            };
            var equipment = new Equipment(
                2,
                "Washer",
                "WM-21",
                100,
                EquipmentStatus.Active
            )
            {
                Id = 21,
            };
            var order = new Order(
                2,
                7,
                "OD-1001",
                100,
                100,
                OrderStatus.Pending,
                note: "original",
                tariffId: TariffId
            )
            {
                Id = OrderId,
                OrderItems =
                [
                    new OrderItem
                    {
                        ServiceId = ServiceId,
                        UnitRelationId = UnitRelationId,
                        Price = 100,
                        Quantity = 1,
                        ServiceName = "Wash",
                        UnitRelationName = "Kg",
                        UnitPrice = 90,
                        ProcessingTime = 30,
                    },
                ],
            };
            context.AddRange(
                staff,
                category,
                service,
                unit,
                tariff,
                serviceTariff,
                equipment,
                order
            );
            await context.SaveChangesAsync();
        }
    }

    private sealed class OrderReadGate
    {
        private readonly TaskCompletionSource _read = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private readonly TaskCompletionSource _release = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public Task WaitUntilReadAsync() => _read.Task.WaitAsync(TimeSpan.FromSeconds(15));

        public void Release() => _release.TrySetResult();

        public async Task AfterReadAsync(CancellationToken cancellationToken)
        {
            _read.TrySetResult();
            await _release.Task.WaitAsync(cancellationToken);
        }
    }

    private sealed class BlockingOrderReadUnitOfWork(
        IUnitOfWork inner,
        OrderReadGate gate
    ) : IUnitOfWork
    {
        public DbTransaction? CurrentTransaction { get; set; }

        public IAsyncRepository<TEntity> Repository<TEntity>(bool isCached = false)
            where TEntity : class => inner.Repository<TEntity>(isCached);

        public IRepositoryFunction<TEntity> RepositoryFunction<TEntity>()
            where TEntity : class => inner.RepositoryFunction<TEntity>();

        public IDynamicSpecificationRepository<TEntity> DynamicReadOnlyRepository<TEntity>(
            bool isCached = false
        )
            where TEntity : class
        {
            IDynamicSpecificationRepository<TEntity> repository =
                inner.DynamicReadOnlyRepository<TEntity>(isCached);
            return typeof(TEntity) == typeof(Order)
                ? (IDynamicSpecificationRepository<TEntity>)(object)new BlockingOrderRepository(
                    (IDynamicSpecificationRepository<Order>)(object)repository,
                    gate
                )
                : repository;
        }

        public ISpecificationRepository<TEntity> ReadOnlyRepository<TEntity>(
            bool isCached = false
        )
            where TEntity : class => inner.ReadOnlyRepository<TEntity>(isCached);

        public Task<DbTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default
        ) => inner.BeginTransactionAsync(cancellationToken);

        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            inner.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            inner.RollbackAsync(cancellationToken);

        public int ExecuteSqlCommand(string sql, params object[] parameters) =>
            inner.ExecuteSqlCommand(sql, parameters);

        public Task<int> ExecuteSqlCommandAsync(
            string sql,
            object[] parameters,
            CancellationToken cancellationToken = default
        ) => inner.ExecuteSqlCommandAsync(sql, parameters, cancellationToken);

        public Task SaveAsync(CancellationToken cancellationToken = default) =>
            inner.SaveAsync(cancellationToken);

        public IQueryable<TEntity> CallPostgreSqlFunction<TEntity>(
            string functionName,
            object[] parameters
        )
            where TEntity : class =>
            inner.CallPostgreSqlFunction<TEntity>(functionName, parameters);

        public void Dispose() => inner.Dispose();
    }

    private sealed class BlockingOrderRepository(
        IDynamicSpecificationRepository<Order> inner,
        OrderReadGate gate
    ) : IDynamicSpecificationRepository<Order>
    {
        public async Task<Order?> FindByConditionAsync(
            ISpecification<Order> spec,
            CancellationToken cancellationToken = default
        )
        {
            Order? result = await inner.FindByConditionAsync(spec, cancellationToken);
            await gate.AfterReadAsync(cancellationToken);
            return result;
        }

        public Task<TResult?> FindByConditionAsync<TResult>(
            ISpecification<Order> spec,
            Expression<Func<Order, TResult>> mappingResult,
            CancellationToken cancellationToken = default
        )
            where TResult : class =>
            inner.FindByConditionAsync(spec, mappingResult, cancellationToken);

        public Task<IList<Order>> ListAsync(
            ISpecification<Order> spec,
            QueryParamRequest queryParam,
            CancellationToken cancellationToken = default
        ) => inner.ListAsync(spec, queryParam, cancellationToken);

        public Task<IList<TResult>> ListAsync<TResult>(
            ISpecification<Order> spec,
            QueryParamRequest queryParam,
            Expression<Func<Order, TResult>> mappingResult,
            CancellationToken cancellationToken = default
        )
            where TResult : class =>
            inner.ListAsync(spec, queryParam, mappingResult, cancellationToken);

        public Task<PaginationResponse<TResult>> PagedListAsync<TResult>(
            ISpecification<Order> spec,
            QueryParamRequest queryParam,
            Expression<Func<Order, TResult>> mappingResult,
            CancellationToken cancellationToken = default
        ) => inner.PagedListAsync(spec, queryParam, mappingResult, cancellationToken);

        public Task<PaginationResponse<TResult>> CursorPagedListAsync<TResult>(
            ISpecification<Order> spec,
            QueryParamRequest queryParam,
            Expression<Func<Order, TResult>> mappingResult,
            string? uniqueSort = null,
            CancellationToken cancellationToken = default
        )
            where TResult : class =>
            inner.CursorPagedListAsync(
                spec,
                queryParam,
                mappingResult,
                uniqueSort,
                cancellationToken
            );
    }
}
