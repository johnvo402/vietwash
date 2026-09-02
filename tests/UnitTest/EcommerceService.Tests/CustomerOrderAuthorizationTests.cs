using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Application.Common.Auth;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Orders.Command.Update;
using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Common;
using Application.Feature.Orders.Queries.Detail;
using Application.Feature.Orders.Queries.DetailByCode;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Application.Feature.Orders.Queries.List;
using Application.Feature.Orders.Queries.TotalOrderByStaff;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Vouchers;
using Infrastructure.Constants;
using Microsoft.AspNetCore.Http;
using Moq;
using Presentation.Endpoints.Orders;
using Shared.Kernel.Common.Specs.Interfaces;

namespace EcommerceService.Tests;

public class CustomerOrderAuthorizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CustomerList_UsesOwnershipRegardlessOfEmptyOrBroadBranches(bool broadBranches)
    {
        IUnitOfWork unitOfWork = ListUnitOfWork(CustomerOrders());
        ListOrderHandler handler = new(
            unitOfWork,
            Actor(ROLE.CUSTOMER, 100, broadBranches ? ["1", "2", "3"] : [])
        );

        var result = await handler.Handle(new ListOrderQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal([10L, 20L], result.Value!.Data!.Select(order => order.Id));
    }

    [Fact]
    public async Task CustomerList_OptionalBranchIsOnlyAFilterOnOwnedOrders()
    {
        ListOrderHandler handler = new(
            ListUnitOfWork(CustomerOrders()),
            Actor(ROLE.CUSTOMER, 100, [])
        );

        var result = await handler.Handle(new ListOrderQuery { BranchId = "2" }, default);

        Assert.True(result.IsSuccess);
        Assert.Equal([20L], result.Value!.Data!.Select(order => order.Id));
    }

    [Fact]
    public async Task StaffList_RemainsBranchScopedRegardlessOfCustomerId()
    {
        ListOrderHandler handler = new(
            ListUnitOfWork(CustomerOrders()),
            Actor(ROLE.STAFF, 999, ["1"])
        );

        var result = await handler.Handle(new ListOrderQuery(), default);

        Assert.True(result.IsSuccess);
        Assert.Equal([10L, 30L], result.Value!.Data!.Select(order => order.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("UNKNOWN")]
    public async Task UnknownRole_ListIsForbiddenBeforeQuerying(string? role)
    {
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        ListOrderHandler handler = new(unitOfWork.Object, Actor(role, 100, ["1", "2"]));

        var result = await handler.Handle(new ListOrderQuery(), default);

        Assert.Equal(403, result.Error?.Status);
        unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CustomerWithNoBranches_CanReadOwnOrderDetail()
    {
        Order order = CustomerOrders()[1];
        var (unitOfWork, details) = DetailUnitOfWork(order);
        GetOrderDetailHandler handler = new(unitOfWork.Object, Actor(ROLE.CUSTOMER, 100, []));

        var result = await handler.Handle(new GetOrderDetailQuery { OrderId = order.Id }, default);

        Assert.True(result.IsSuccess);
        details.Verify(
            repository =>
                repository.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<Expression<Func<Order, GetOrderDetailResponse>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CustomerCannotReadAnotherCustomersOrderEvenWithBroadBranches(
        bool broadBranches
    )
    {
        Order order = CustomerOrders()[2];
        var (unitOfWork, details) = DetailUnitOfWork(order);
        GetOrderDetailHandler handler = new(
            unitOfWork.Object,
            Actor(ROLE.CUSTOMER, 100, broadBranches ? ["1", "2", "3"] : [])
        );

        var result = await handler.Handle(new GetOrderDetailQuery { OrderId = order.Id }, default);

        Assert.Equal(404, result.Error?.Status);
        details.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(ROLE.ADMIN)]
    [InlineData(ROLE.MANAGER)]
    [InlineData(ROLE.STAFF)]
    public void StaffActorPolicy_UsesBranchAndIgnoresCustomerOwnership(string role)
    {
        Assert.True(OrderActorAccess.CanReadOrder(role, 999, ["1"], 100, 1));
        Assert.False(OrderActorAccess.CanReadOrder(role, 100, ["1"], 100, 2));
        Assert.True(OrderActorAccess.CanOperateOrder(role, ["1"], 1));
        Assert.False(OrderActorAccess.CanOperateOrder(role, ["1"], 2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("UNKNOWN")]
    public void UnknownActorPolicy_DeniesReadsAndOperations(string? role)
    {
        Assert.False(OrderActorAccess.CanReadOrder(role, 100, ["1"], 100, 1));
        Assert.False(OrderActorAccess.CanOperateOrder(role, ["1"], 1));
    }

    [Fact]
    public async Task CustomerCannotCreateOrUpdateOrdersEvenWithMatchingBranch()
    {
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        ICurrentAccount customer = Actor(ROLE.CUSTOMER, 100, ["1"]);
        CreateOrderHandler create = new(
            unitOfWork.Object,
            customer,
            new OrgSetting(),
            Mock.Of<IEncryptionService>(),
            Mock.Of<IQrGenerator>()
        );
        UpdateOrderHandler update = new(unitOfWork.Object, customer);

        var createResult = await create.Handle(
            new CreateOrderCommand { BranchId = 1, CustomerId = 100 },
            default
        );
        var updateResult = await update.Handle(
            new UpdateOrderCommand { OrderId = 10, Model = new UpdateOrderModel() },
            default
        );

        Assert.Equal(403, createResult.Error?.Status);
        Assert.Equal(403, updateResult.Error?.Status);
        unitOfWork.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.InProgress, null)]
    [InlineData(OrderStatus.Processed, OrderStatus.Completed, PaymentMethod.Cash)]
    [InlineData(OrderStatus.Processed, OrderStatus.Cancelled, null)]
    public async Task CustomerCannotStartCompleteOrCancelOrderBeforeAnySideEffect(
        OrderStatus currentStatus,
        OrderStatus target,
        PaymentMethod? paymentMethod
    )
    {
        Order order = new(1, 999, "ORD-10", 100, 100, currentStatus, customerId: 100);
        Equipment equipment = new(1, "Washer", "EQ-1", 100, EquipmentStatus.Active);
        VoucherCustomer voucher = new()
        {
            CustomerId = 100,
            VoucherId = 5,
            IsUsed = true,
        };
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        Mock<IOrderPaymentLinkClient> paymentClient = new(MockBehavior.Strict);
        UpdateStatusHandler handler = new(
            unitOfWork.Object,
            Actor(ROLE.CUSTOMER, 100, ["1"]),
            paymentClient.Object
        );

        Result result = await handler.Handle(
            new UpdateStatusCommand
            {
                OrderId = order.Id.ToString(),
                Model = new OrderUpdateStatus
                {
                    Status = target,
                    PaymentMethod = paymentMethod,
                    CancellationReason =
                        target == OrderStatus.Cancelled ? "Customer request" : null,
                    OrderEquipments =
                        target == OrderStatus.InProgress
                            ? [new() { EquipmentId = equipment.Id }]
                            : null,
                },
            },
            default
        );

        Assert.Equal(403, result.Error?.Status);
        Assert.Equal(currentStatus, order.Status);
        Assert.False(equipment.Using);
        Assert.True(voucher.IsUsed);
        Assert.Empty(order.UncommittedEvents);
        unitOfWork.VerifyNoOtherCalls();
        paymentClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CustomerCannotUseCashierLookupTotalsOrPaymentLinks()
    {
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        Mock<IEncryptionService> encryption = new(MockBehavior.Strict);
        Mock<IOrderPaymentLinkClient> paymentClient = new(MockBehavior.Strict);
        ICurrentAccount customer = Actor(ROLE.CUSTOMER, 100, ["1", "2", "3"]);

        var byCode = await new GetOrderDetailByCodeHandler(
            unitOfWork.Object,
            encryption.Object,
            customer
        ).Handle(new GetOrderDetailByCodeQuery { Code = "guessed-code" }, default);
        var totals = await new TotalOrderByStaffHandler(unitOfWork.Object, customer).Handle(
            new TotalOrderByStaffQuery { StaffId = 100 },
            default
        );
        var payment = await new GetLinkPaymentHandler(
            paymentClient.Object,
            unitOfWork.Object,
            customer
        ).Handle(new GetLinkPaymentQuery { OrderId = 10 }, default);

        Assert.Equal(403, byCode.Error?.Status);
        Assert.Equal(403, totals.Error?.Status);
        Assert.Equal(403, payment.Error?.Status);
        unitOfWork.VerifyNoOtherCalls();
        encryption.VerifyNoOtherCalls();
        paymentClient.VerifyNoOtherCalls();
    }

    [Fact]
    public void OperationalEndpointsDeclareStaffRolesWhileOwnershipReadsRemainAuthenticated()
    {
        Type[] staffEndpoints =
        [
            typeof(CreateOrderEndpoint),
            typeof(UpdateOrderEndpoint),
            typeof(Presentation.Endpoints.Orders.UpdateStatus),
            typeof(GetGetLinkPaymentEndpoint),
            typeof(GetOrderDetailByCodeEndpoint),
            typeof(TotalOrderByStaffEndpoint),
        ];
        foreach (Type endpoint in staffEndpoints)
        {
            AuthorizeByAttribute authorization = AuthorizationFor(endpoint);
            AuthorizeModel model = JsonSerializer.Deserialize<AuthorizeModel>(
                authorization.Value,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            )!;
            Assert.Equal([ROLE.ADMIN, ROLE.MANAGER, ROLE.STAFF], model.Roles);
        }

        Assert.Equal(string.Empty, AuthorizationFor(typeof(ListOrderEndpoint)).Value);
        Assert.Equal(string.Empty, AuthorizationFor(typeof(GetOrderDetailEndpoint)).Value);
    }

    private static AuthorizeByAttribute AuthorizationFor(Type endpoint) =>
        endpoint.GetMethod("HandleAsync")!.GetCustomAttribute<AuthorizeByAttribute>()!;

    private static Order[] CustomerOrders() =>
        [
            new(1, 999, "A", 100, 100, OrderStatus.Processed, customerId: 100) { Id = 10 },
            new(2, 999, "B", 100, 100, OrderStatus.Processed, customerId: 100) { Id = 20 },
            new(1, 999, "C", 100, 100, OrderStatus.Processed, customerId: 200) { Id = 30 },
        ];

    private static IUnitOfWork ListUnitOfWork(IEnumerable<Order> seed)
    {
        Mock<IDynamicSpecificationRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(repository =>
                repository.PagedListAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<QueryParamRequest>(),
                    It.IsAny<Expression<Func<Order, ListOrderResponse>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    ISpecification<Order> specification,
                    QueryParamRequest query,
                    Expression<Func<Order, ListOrderResponse>> _,
                    CancellationToken __
                ) =>
                {
                    List<ListOrderResponse> matches = seed.Where(order =>
                            specification.Criteria.All(criteria =>
                                criteria.Criteria.Compile()(order)
                            )
                        )
                        .Select(order => new ListOrderResponse { Id = order.Id })
                        .ToList();
                    return Task.FromResult(
                        new PaginationResponse<ListOrderResponse>(
                            matches,
                            1,
                            query.Page,
                            query.PageSize
                        )
                    );
                }
            );
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(unit => unit.DynamicReadOnlyRepository<Order>(false))
            .Returns(orders.Object);
        return unitOfWork.Object;
    }

    private static (
        Mock<IUnitOfWork>,
        Mock<IDynamicSpecificationRepository<Order>>
    ) DetailUnitOfWork(Order order)
    {
        Mock<IAsyncRepository<Order>> lookup = new(MockBehavior.Strict);
        lookup
            .Setup(repository =>
                repository.FindByConditionAsync(
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Expression<Func<Order, OrderBranchReference>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(
                (
                    Expression<Func<Order, bool>> criteria,
                    Expression<Func<Order, OrderBranchReference>> selector,
                    CancellationToken _
                ) => Task.FromResult(criteria.Compile()(order) ? selector.Compile()(order) : null)
            );
        Mock<IDynamicSpecificationRepository<Order>> details = new(MockBehavior.Strict);
        details
            .Setup(repository =>
                repository.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<Expression<Func<Order, GetOrderDetailResponse>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new GetOrderDetailResponse { Id = order.Id });
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork.Setup(unit => unit.Repository<Order>(false)).Returns(lookup.Object);
        unitOfWork
            .Setup(unit => unit.DynamicReadOnlyRepository<Order>(false))
            .Returns(details.Object);
        return (unitOfWork, details);
    }

    private static ICurrentAccount Actor(string? role, long id, IEnumerable<string> branches) =>
        new TestCurrentAccount
        {
            Id = id,
            Session = new UserAuth
            {
                Id = id,
                Role = role,
                Branches = branches,
            },
        };

    private sealed class TestCurrentAccount : ICurrentAccount
    {
        public long? Id { get; init; }
        public string? ClientIp => null;
        public UserAuth? Session { get; init; }

        public void SetClientIp(HttpContext httpContext) { }

        public Task SetClaimPrinciple(ClaimsPrincipal user) => Task.CompletedTask;
    }
}
