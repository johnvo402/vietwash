using System.Collections;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Application.Common.Auth;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Orders.Queries.Preview;
using Application.Features.Users.Queries.Detail;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Responses;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services;
using Domain.Aggregates.Tariffs;
using Domain.Aggregates.Users;
using Domain.Aggregates.Vouchers;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Shared.Kernel.Common.Specs.Interfaces;

namespace EcommerceService.Tests;

public class CashierReliabilityTests
{
    [Theory]
    [InlineData("CUSTOMER", true, false, true)]
    [InlineData("STAFF", true, false, false)]
    [InlineData("MANAGER", true, false, false)]
    [InlineData("ADMIN", true, false, false)]
    [InlineData("CUSTOMER", false, false, false)]
    [InlineData("CUSTOMER", true, true, false)]
    public async Task PreviewAndCreate_UseIdenticalCustomerEligibility(
        string role,
        bool active,
        bool disabled,
        bool accepted
    )
    {
        var h = new Harness();
        h.Customer.Update(
            role: role,
            status: active ? ActivationStatus.Active : ActivationStatus.Inactive
        );
        h.Customer.Disabled = disabled;
        var preview = await h.Preview();
        h.AssertReadOnly();
        var create = await h.Create();
        Assert.Equal(accepted, preview.IsSuccess);
        Assert.Equal(accepted, create.IsSuccess);
        if (!accepted)
        {
            Assert.Equal(404, preview.Error!.Status);
            Assert.Equal(preview.Error.Title, create.Error!.Title);
            Assert.Null(h.Persisted);
            h.Unit.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(false, 0, 0)]
    [InlineData(true, 50, 8)]
    [InlineData(false, 15, 12)]
    public async Task Preview_ReconcilesWithCreatedPersistedOrder_WithoutWrites(
        bool fixedDiscount,
        decimal discount,
        int vat
    )
    {
        var h = new Harness(fixedDiscount, discount, vat);
        var preview = await h.Preview();
        Assert.True(preview.IsSuccess);
        h.AssertReadOnly();
        Assert.Equal(400m, preview.Value!.Amount);
        Assert.Equal(new[] { 125m, 25m }, preview.Value.OrderItems.Select(x => x.UnitPrice));
        Assert.Equal(new[] { 375m, 25m }, preview.Value.OrderItems.Select(x => x.LineAmount));

        var created = await h.Create();
        Assert.True(created.IsSuccess);
        Assert.NotNull(h.Persisted);
        Assert.Equal(preview.Value.Amount, h.Persisted.Amount);
        Assert.Equal(preview.Value.DiscountAmount, created.Value!.DiscountAmount);
        Assert.Equal(preview.Value.VatAmount, h.Persisted.VatAmount);
        Assert.Equal(preview.Value.Total, h.Persisted.Total);
        Assert.Equal(preview.Value.Total, created.Value.Total);
        Assert.Equal(preview.Value.NetBeforeVat, created.Value.Total - created.Value.VatAmount);
        Assert.Equal(vat, created.Value.Vat);
        Assert.Equal(discount == 0 ? 0 : 1, h.Claims);
        Assert.Equal(0, h.Persisted.Point);
    }

    [Fact]
    public async Task CreateRecalculatesAfterPreview_WhenTariffChanges()
    {
        var h = new Harness();
        var before = await h.Preview();
        h.Prices[0].Price = 200m;
        var created = await h.Create();
        Assert.Equal(400m, before.Value!.Total);
        Assert.Equal(625m, created.Value!.Total);
    }

    [Theory]
    [InlineData("fixed-over-subtotal")]
    [InlineData("invalid-tariff")]
    [InlineData("cross-branch-tariff")]
    [InlineData("inactive-service")]
    [InlineData("cross-branch-service")]
    [InlineData("inactive-unit")]
    [InlineData("duplicate")]
    [InlineData("invalid-voucher")]
    [InlineData("expired-voucher")]
    [InlineData("inactive-voucher")]
    [InlineData("used-voucher")]
    [InlineData("unassigned-voucher")]
    public async Task PreviewAndCreate_RejectSameInvalidSelection(string scenario)
    {
        var h = new Harness(true, 10, 8);
        switch (scenario)
        {
            case "fixed-over-subtotal":
                h.Voucher.DiscountValue = 1000;
                break;
            case "invalid-tariff":
                h.Request.TariffId = 999;
                break;
            case "cross-branch-tariff":
                h.Tariff.BranchId = 2;
                break;
            case "inactive-service":
                h.Service.Status = ActivationStatus.Inactive;
                break;
            case "cross-branch-service":
                h.Service.BranchId = 2;
                break;
            case "inactive-unit":
                h.Prices[0].UnitRelation.Status = ActivationStatus.Inactive;
                break;
            case "duplicate":
                h.Request.OrderItems.Add(h.Request.OrderItems[0]);
                break;
            case "invalid-voucher":
                h.Request.VoucherCode = "MISSING";
                break;
            case "expired-voucher":
                h.Voucher.EndAt = DateTimeOffset.UtcNow.AddDays(-1);
                break;
            case "inactive-voucher":
                h.Voucher.Status = ActivationStatus.Inactive;
                break;
            case "used-voucher":
                h.Voucher.VoucherCustomers.Single().IsUsed = true;
                break;
            case "unassigned-voucher":
                h.Voucher.VoucherCustomers.Single().CustomerId = 999;
                break;
        }
        var preview = await h.Preview();
        Assert.True(preview.IsFailure);
        h.AssertReadOnly(allowAlreadyUsed: scenario == "used-voucher");
        var created = await h.Create();
        Assert.True(created.IsFailure);
        Assert.Equal(preview.Error!.Status, created.Error!.Status);
        Assert.Equal(preview.Error.Title, created.Error.Title);
        Assert.Null(h.Persisted);
        Assert.Equal(0, h.Claims);
    }

    [Fact]
    public async Task VoucherClaimRace_RemainsFinalAuthority_AfterSuccessfulPreview()
    {
        var h = new Harness(true, 10);
        Assert.True((await h.Preview()).IsSuccess);
        h.ClaimSucceeds = false;
        var created = await h.Create();
        Assert.True(created.IsFailure);
        Assert.Contains("already used", created.Error!.Title);
        Assert.Null(h.Persisted);
        h.Unit.Verify(x => x.RollbackAsync(default), Times.Once);
    }

    [Theory]
    [InlineData("CUSTOMER", "1")]
    [InlineData("UNKNOWN", "1")]
    [InlineData("STAFF", "2")]
    public async Task Preview_DeniesUnauthorizedActorsBeforeQueries(string role, string branch)
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var handler = new PreviewOrderHandler(unit.Object, Actor(role, branch), new OrgSetting());
        var result = await handler.Handle(new PreviewOrderQuery { BranchId = 1 }, default);
        Assert.Equal(403, result.Error!.Status);
        unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("CUSTOMER", true, false, 200)]
    [InlineData("CUSTOMER", false, false, 404)]
    [InlineData("STAFF", true, false, 404)]
    [InlineData("MANAGER", true, false, 404)]
    [InlineData("ADMIN", true, false, 404)]
    [InlineData("CUSTOMER", true, true, 404)]
    public async Task CustomerLookup_ReturnsOnlyActiveCustomers(
        string role,
        bool active,
        bool disabled,
        int expected
    )
    {
        var user = new User("Customer", null, "0901234567", role, "C501")
        {
            Id = 501,
            Status = active ? ActivationStatus.Active : ActivationStatus.Inactive,
            Disabled = disabled,
        };
        var users = new Mock<IAsyncRepository<User>>(MockBehavior.Strict);
        users
            .Setup(x =>
                x.FindByConditionAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, GetCustomerResponse>>>(),
                    default
                )
            )
            .Returns(
                (
                    Expression<Func<User, bool>> predicate,
                    Expression<Func<User, GetCustomerResponse>> projection,
                    CancellationToken _
                ) => Task.FromResult(predicate.Compile()(user) ? projection.Compile()(user) : null)
            );
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        unit.Setup(x => x.Repository<User>(false)).Returns(users.Object);
        var handler = new GetCustomerHandler(unit.Object, Actor());
        var result = await handler.Handle(new GetCustomerQuery(501), default);
        Assert.Equal(expected, result.IsSuccess ? 200 : result.Error!.Status);
        if (result.IsSuccess)
            Assert.Equal(501, result.Value!.Id);
        Assert.True((await handler.Handle(new GetCustomerQuery(502), default)).IsFailure);
    }

    [Theory]
    [InlineData("CUSTOMER")]
    [InlineData("UNKNOWN")]
    public async Task CustomerLookup_IsStaffOnly(string role)
    {
        var unit = new Mock<IUnitOfWork>(MockBehavior.Strict);
        var result = await new GetCustomerHandler(unit.Object, Actor(role)).Handle(
            new(501),
            default
        );
        Assert.Equal(403, result.Error!.Status);
        unit.VerifyNoOtherCalls();
    }

    [Fact]
    public void PreviewContract_OnlyAcceptsSelectionInputs_EndpointsHaveStaffMetadata()
    {
        Assert.Equal(
            new[] { "BranchId", "CustomerId", "OrderItems", "TariffId", "VoucherCode" },
            typeof(PreviewOrderQuery).GetProperties().Select(x => x.Name).Order()
        );
        foreach (
            var type in new[]
            {
                typeof(Presentation.Endpoints.Orders.PreviewOrderEndpoint),
                typeof(Presentation.Endpoints.User.GetCustomerEndpoint),
            }
        )
        {
            var attribute = type.GetMethod("HandleAsync")!
                .GetCustomAttribute<AuthorizeByAttribute>()!;
            var model = JsonSerializer.Deserialize<AuthorizeModel>(
                attribute.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            Assert.Equal(new[] { "ADMIN", "MANAGER", "STAFF" }, model!.Roles);
        }
    }

    private static ICurrentAccount Actor(string role = "STAFF", string branch = "1") =>
        Mock.Of<ICurrentAccount>(x =>
            x.Id == 7
            && x.Session
                == new UserAuth
                {
                    Id = 7,
                    Role = role,
                    Branches = new[] { branch },
                }
        );

    private sealed class Harness
    {
        public Mock<IUnitOfWork> Unit { get; } = new(MockBehavior.Strict);
        public Tariff Tariff { get; } = new("Standard", 1, ActivationStatus.Active) { Id = 5 };
        public Service Service { get; } = new(1, 1, "Wash", ActivationStatus.Active) { Id = 10 };
        public List<ServiceTariff> Prices { get; }
        public Voucher Voucher { get; }
        public PreviewOrderQuery Request { get; }
        public Order? Persisted { get; private set; }
        public int Claims { get; private set; }
        public bool ClaimSucceeds { get; set; } = true;
        public User Customer { get; } =
            new("Customer", null, "0901234567", "CUSTOMER", "C501")
            {
                Id = 501,
                Status = ActivationStatus.Active,
            };
        private readonly OrgSetting settings;

        public Harness(bool fixedDiscount = false, decimal discount = 0, int vat = 0)
        {
            settings = new OrgSetting { VatPercent = vat };
            Prices = new[] { (2L, 125m), (3L, 25m) }
                .Select(pair => new ServiceTariff
                {
                    TariffId = 5,
                    ServiceId = 10,
                    Service = Service,
                    UnitRelationId = pair.Item1,
                    Price = pair.Item2,
                    UnitRelation = new UnitRelation
                    {
                        Id = pair.Item1,
                        ServiceId = 10,
                        Name = $"Unit {pair.Item1}",
                        Price = 5,
                        Status = ActivationStatus.Active,
                    },
                })
                .ToList();
            Voucher = new Voucher
            {
                Id = 20,
                Code = "SAVE",
                DiscountFixed = fixedDiscount,
                DiscountValue = discount,
                Status = ActivationStatus.Active,
                VoucherCustomers = [new() { CustomerId = 501, VoucherId = 20 }],
            };
            Request = new PreviewOrderQuery
            {
                BranchId = 1,
                TariffId = 5,
                CustomerId = 501,
                VoucherCode = discount > 0 ? "SAVE" : null,
                OrderItems =
                [
                    new()
                    {
                        ServiceId = 10,
                        UnitRelationId = 2,
                        Quantity = 3,
                    },
                    new()
                    {
                        ServiceId = 10,
                        UnitRelationId = 3,
                        Quantity = 1,
                    },
                ],
            };
            ReadRepository(new[] { Customer });
            ReadRepository(new[] { Tariff });
            ReadRepository(Prices);
            ReadRepository(new[] { Voucher });
        }

        private Mock<IAsyncRepository<T>> ReadRepository<T>(
            IEnumerable<T> rows,
            Func<int>? write = null
        )
            where T : class
        {
            var repo = new Mock<IAsyncRepository<T>>(MockBehavior.Strict);
            repo.Setup(x => x.QueryAsync(It.IsAny<Expression<Func<T, bool>>?>()))
                .Returns(
                    (Expression<Func<T, bool>>? predicate) =>
                        new AsyncRows<T>(
                            predicate is null ? rows : rows.Where(predicate.Compile()),
                            write
                        )
                );
            repo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<T, bool>>>(), default))
                .Returns(
                    (Expression<Func<T, bool>> predicate, CancellationToken _) =>
                        Task.FromResult(rows.Any(predicate.Compile()))
                );
            Unit.Setup(x => x.Repository<T>(false)).Returns(repo.Object);
            return repo;
        }

        public ValueTask<Contracts.ApiWrapper.Result<PreviewOrderResponse>> Preview() =>
            new PreviewOrderHandler(Unit.Object, Actor(), settings).Handle(Request, default);

        public void AssertReadOnly(bool allowAlreadyUsed = false)
        {
            Assert.Null(Persisted);
            Assert.Equal(0, Claims);
            Assert.Equal(allowAlreadyUsed, Voucher.VoucherCustomers.Single().IsUsed);
            Assert.Empty(Voucher.UncommittedEvents);
            Assert.All(
                Unit.Invocations,
                invocation => Assert.Equal("Repository", invocation.Method.Name)
            );
        }

        public ValueTask<Contracts.ApiWrapper.Result<CreateOrderResponse>> Create()
        {
            Unit.Setup(x => x.BeginTransactionAsync(default))
                .ReturnsAsync(Mock.Of<DbTransaction>());
            Unit.Setup(x => x.SaveAsync(default)).Returns(Task.CompletedTask);
            Unit.Setup(x => x.CommitAsync(default)).Returns(Task.CompletedTask);
            Unit.Setup(x => x.RollbackAsync(default)).Returns(Task.CompletedTask);
            ReadRepository(
                Voucher.VoucherCustomers,
                () =>
                {
                    Claims++;
                    return ClaimSucceeds ? 1 : 0;
                }
            );
            var orders = new Mock<IAsyncRepository<Order>>(MockBehavior.Strict);
            orders
                .Setup(x => x.AddAsync(It.IsAny<Order>(), default))
                .Returns(
                    (Order order, CancellationToken _) =>
                    {
                        Persisted = order;
                        foreach (var item in order.OrderItems)
                            item.Service = Service;
                        return Task.FromResult(order);
                    }
                );
            Unit.Setup(x => x.Repository<Order>(false)).Returns(orders.Object);
            var details = new Mock<IDynamicSpecificationRepository<Order>>(MockBehavior.Strict);
            details
                .Setup(x =>
                    x.FindByConditionAsync(
                        It.IsAny<ISpecification<Order>>(),
                        It.IsAny<Expression<Func<Order, CreateOrderResponse>>>(),
                        default
                    )
                )
                .Returns(
                    (
                        ISpecification<Order> _,
                        Expression<Func<Order, CreateOrderResponse>> selector,
                        CancellationToken _
                    ) => Task.FromResult<CreateOrderResponse?>(selector.Compile()(Persisted!))
                );
            Unit.Setup(x => x.DynamicReadOnlyRepository<Order>(false)).Returns(details.Object);
            return new CreateOrderHandler(
                Unit.Object,
                Actor(),
                settings,
                Mock.Of<IEncryptionService>(),
                Mock.Of<IQrGenerator>()
            ).Handle(
                new CreateOrderCommand
                {
                    BranchId = Request.BranchId,
                    TariffId = Request.TariffId,
                    CustomerId = Request.CustomerId,
                    VoucherCode = Request.VoucherCode,
                    OrderItems = Request.OrderItems,
                },
                default
            );
        }
    }

    // Runs the real query expressions and projections without a database; writes are rejected by default.
    internal sealed class AsyncRows<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly Func<int>? write;

        public AsyncRows(IEnumerable<T> rows, Func<int>? write = null)
            : base(rows)
        {
            this.write = write;
        }

        public AsyncRows(Expression expression, Func<int>? write)
            : base(expression)
        {
            this.write = write;
        }

        IQueryProvider IQueryable.Provider => new AsyncProvider(this, write);

        public IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default
        ) => new AsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    private sealed class AsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class AsyncProvider(IQueryProvider inner, Func<int>? write) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => throw new NotSupportedException();

        public IQueryable<T> CreateQuery<T>(Expression expression) =>
            new AsyncRows<T>(expression, write);

        public object? Execute(Expression expression) => inner.Execute(expression);

        public T Execute<T>(Expression expression) => inner.Execute<T>(expression);

        public TResult ExecuteAsync<TResult>(
            Expression expression,
            CancellationToken cancellationToken = default
        )
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            object? value = expression is MethodCallExpression { Method.Name: "ExecuteUpdate" }
                ? (write ?? throw new InvalidOperationException("Unexpected preview write"))()
                : inner.Execute(expression);
            return (TResult)
                typeof(Task)
                    .GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, [value])!;
        }
    }
}
