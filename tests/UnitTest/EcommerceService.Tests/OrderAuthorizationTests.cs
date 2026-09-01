using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
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
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Application.Common.Interfaces.Services.Encryptions;
using Contracts.Application.Common.Interfaces.UnitOfWorks;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Contracts.Infrastructure.Common;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Users;
using Mediator;
using Moq;
using Net.payOS.Types;
using Presentation.Endpoints.Orders;
using Presentation.Endpoints.Webhooks;
using Shared.Kernel.Common.Specs.Interfaces;

namespace EcommerceService.Tests;

public class OrderAuthorizationTests
{
    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void BranchPolicy_AuthorizesOnlyParsedSessionBranches(long branchId, bool expected)
    {
        OrderBranchAccess access = OrderBranchAccess.FromSession(["invalid", "1", "1"]);

        Assert.Equal(expected, access.IsAuthorized(branchId));
        Assert.Equal([1L], access.BranchIds);
    }

    [Fact]
    public async Task CreateOrder_UnauthorizedBranchIsForbiddenBeforeAnySideEffect()
    {
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        CreateOrderHandler handler = CreateHandler(unitOfWork.Object, CurrentAccount(["1"]));

        Result<CreateOrderResponse> result = await handler.Handle(
            new CreateOrderCommand
            {
                BranchId = 2,
                CustomerId = 10,
                VoucherCode = "MUST-NOT-BE-CLAIMED",
            },
            default
        );

        AssertForbidden(result.Error);
        unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateOrder_AuthorizedBranchPassesAuthorizationGuard()
    {
        Mock<IAsyncRepository<User>> users = new(MockBehavior.Strict);
        users
            .Setup(x =>
                x.AnyAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(false);
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork.Setup(x => x.Repository<User>(false)).Returns(users.Object);
        CreateOrderHandler handler = CreateHandler(unitOfWork.Object, CurrentAccount(["1"]));

        Result<CreateOrderResponse> result = await handler.Handle(
            new CreateOrderCommand { BranchId = 1, CustomerId = 10 },
            default
        );

        Assert.False(result.IsSuccess);
        Assert.NotEqual(403, result.Error?.Status);
        users.Verify(
            x =>
                x.AnyAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetDetail_UnauthorizedOrderIsForbiddenBeforeDetailedDataLoads()
    {
        (Mock<IUnitOfWork> unitOfWork, Mock<IDynamicSpecificationRepository<Order>> details) =
            BranchLookupUnitOfWork(branchId: 2);
        GetOrderDetailHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result<GetOrderDetailResponse> result = await handler.Handle(
            new GetOrderDetailQuery { OrderId = 20 },
            default
        );

        AssertForbidden(result.Error);
        details.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetDetail_AuthorizedOrderLoadsAndReturnsDetail()
    {
        (Mock<IUnitOfWork> unitOfWork, Mock<IDynamicSpecificationRepository<Order>> details) =
            BranchLookupUnitOfWork(branchId: 1);
        details
            .Setup(x =>
                x.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<Expression<Func<Order, GetOrderDetailResponse>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new GetOrderDetailResponse());
        GetOrderDetailHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result<GetOrderDetailResponse> result = await handler.Handle(
            new GetOrderDetailQuery { OrderId = 10 },
            default
        );

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetDetailByCode_UnauthorizedOrderIsForbiddenBeforeDetailedDataLoads()
    {
        (Mock<IUnitOfWork> unitOfWork, Mock<IDynamicSpecificationRepository<Order>> details) =
            BranchLookupUnitOfWork(branchId: 2);
        Mock<IEncryptionService> encryption = new(MockBehavior.Strict);
        encryption.Setup(x => x.Decrypt("encrypted")).Returns("ORD-20");
        GetOrderDetailByCodeHandler handler = new(
            unitOfWork.Object,
            encryption.Object,
            CurrentAccount(["1"])
        );

        Result<GetOrderDetailByCodeResponse> result = await handler.Handle(
            new GetOrderDetailByCodeQuery { Code = "encrypted" },
            default
        );

        AssertForbidden(result.Error);
        details.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateOrder_UnauthorizedBranchCannotChangeOrder()
    {
        Order order = OrderAtBranch(2, OrderStatus.Pending);
        (Mock<IUnitOfWork> unitOfWork, _) = TransactionalOrderUnitOfWork(order);
        UpdateOrderHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));
        DateTimeOffset originalDeliveryTime = order.DeliveryTime;

        Result result = await handler.Handle(
            new UpdateOrderCommand
            {
                OrderId = 20,
                Model = new UpdateOrderModel
                {
                    Note = "must not apply",
                    DeliveryTime = originalDeliveryTime.AddDays(5),
                },
            },
            default
        );

        AssertForbidden(result.Error);
        Assert.Equal(string.Empty, order.Note);
        Assert.Equal(originalDeliveryTime, order.DeliveryTime);
        unitOfWork.Verify(x => x.Repository<Order>(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task UpdateOrder_AuthorizedBranchPassesAuthorizationGuard()
    {
        Order order = OrderAtBranch(1, OrderStatus.Completed);
        (Mock<IUnitOfWork> unitOfWork, _) = TransactionalOrderUnitOfWork(order);
        UpdateOrderHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result result = await handler.Handle(
            new UpdateOrderCommand { OrderId = 10, Model = new UpdateOrderModel() },
            default
        );

        Assert.True(result.IsFailure);
        Assert.NotEqual(403, result.Error?.Status);
    }

    [Fact]
    public async Task UpdateStatus_UnauthorizedBranchHasZeroLifecycleSideEffects()
    {
        Order order = OrderAtBranch(2, OrderStatus.Pending);
        Equipment equipment = new(
            2,
            "Washer",
            "EQ-2",
            100,
            Domain.Aggregates.Equipments.Enums.EquipmentStatus.Active
        );
        (Mock<IUnitOfWork> unitOfWork, _) = TransactionalOrderUnitOfWork(order);
        UpdateStatusHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result result = await handler.Handle(
            new UpdateStatusCommand
            {
                OrderId = "20",
                Model = new OrderUpdateStatus
                {
                    Status = OrderStatus.InProgress,
                    OrderEquipments = [new OrderEquipmentSelectionModel { EquipmentId = 2 }],
                },
            },
            default
        );

        AssertForbidden(result.Error);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.False(equipment.Using);
        Assert.Empty(order.UncommittedEvents);
        unitOfWork.Verify(x => x.Repository<Equipment>(It.IsAny<bool>()), Times.Never);
        unitOfWork.Verify(x => x.Repository<InventoryDocument>(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatus_AuthorizedBranchAllowsIdempotentTransition()
    {
        Order order = OrderAtBranch(1, OrderStatus.Processed);
        (Mock<IUnitOfWork> unitOfWork, _) = TransactionalOrderUnitOfWork(
            order,
            commitExpected: true
        );
        UpdateStatusHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result result = await handler.Handle(
            new UpdateStatusCommand
            {
                OrderId = "10",
                Model = new OrderUpdateStatus { Status = OrderStatus.Processed },
            },
            default
        );

        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.Empty(order.UncommittedEvents);
    }

    [Fact]
    public async Task UpdateStatus_UnauthorizedCompletionEmitsNoFinancialEvent()
    {
        Order order = OrderAtBranch(2, OrderStatus.Processed);
        (Mock<IUnitOfWork> unitOfWork, _) = TransactionalOrderUnitOfWork(order);
        UpdateStatusHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result result = await handler.Handle(
            new UpdateStatusCommand
            {
                OrderId = "20",
                Model = new OrderUpdateStatus
                {
                    Status = OrderStatus.Completed,
                    PaymentMethod = PaymentMethod.Card,
                },
            },
            default
        );

        AssertForbidden(result.Error);
        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.Empty(order.UncommittedEvents);
        unitOfWork.Verify(x => x.Repository<Order>(It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task PaymentLink_UnauthorizedBranchNeverInvokesPayOsClient()
    {
        Mock<IOrderPaymentLinkClient> paymentClient = new(MockBehavior.Strict);
        Mock<IUnitOfWork> unitOfWork = PaymentOrderUnitOfWork(branchId: 2);
        GetLinkPaymentHandler handler = new(
            paymentClient.Object,
            unitOfWork.Object,
            CurrentAccount(["1"])
        );

        Result<CreatePaymentResult> result = await handler.Handle(
            new GetLinkPaymentQuery { OrderId = 20, ReturnUrl = "https://return.test" },
            default
        );

        AssertForbidden(result.Error);
        paymentClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PaymentLink_AuthorizedEligibleOrderInvokesPayOsClient()
    {
        CreatePaymentResult payment = PaymentResult();
        Mock<IOrderPaymentLinkClient> paymentClient = new(MockBehavior.Strict);
        paymentClient
            .Setup(x => x.CreatePaymentLinkAsync(It.IsAny<PaymentData>()))
            .ReturnsAsync(payment);
        Mock<IUnitOfWork> unitOfWork = PaymentOrderUnitOfWork(branchId: 1);
        GetLinkPaymentHandler handler = new(
            paymentClient.Object,
            unitOfWork.Object,
            CurrentAccount(["1"])
        );

        Result<CreatePaymentResult> result = await handler.Handle(
            new GetLinkPaymentQuery { OrderId = 10, ReturnUrl = "https://return.test" },
            default
        );

        Assert.True(result.IsSuccess);
        Assert.Same(payment, result.Value);
        paymentClient.Verify(
            x => x.CreatePaymentLinkAsync(It.IsAny<PaymentData>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ListOrder_ExplicitUnauthorizedBranchIsForbiddenWithoutQueryingOrders()
    {
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        ListOrderHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result<PaginationResponse<ListOrderResponse>> result = await handler.Handle(
            new ListOrderQuery { BranchId = "2" },
            default
        );

        AssertForbidden(result.Error);
        unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ListOrder_ExplicitAuthorizedBranchUsesAuthorizedScope()
    {
        Mock<IDynamicSpecificationRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(x =>
                x.PagedListAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<QueryParamRequest>(),
                    It.IsAny<Expression<Func<Order, ListOrderResponse>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new PaginationResponse<ListOrderResponse>([], 0, 1, 100));
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(x => x.DynamicReadOnlyRepository<Order>(false))
            .Returns(orders.Object);
        ListOrderHandler handler = new(unitOfWork.Object, CurrentAccount(["1"]));

        Result<PaginationResponse<ListOrderResponse>> result = await handler.Handle(
            new ListOrderQuery { BranchId = "1" },
            default
        );

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void AllOrderBusinessEndpointsRequireAuthentication()
    {
        Type[] protectedEndpoints =
        [
            typeof(CreateOrderEndpoint),
            typeof(GetGetLinkPaymentEndpoint),
            typeof(GetOrderDetailEndpoint),
            typeof(GetOrderDetailByCodeEndpoint),
            typeof(ListOrderEndpoint),
            typeof(TotalOrderByStaffEndpoint),
            typeof(UpdateOrderEndpoint),
            typeof(Presentation.Endpoints.Orders.UpdateStatus),
        ];

        foreach (Type endpoint in protectedEndpoints)
        {
            MethodInfo method = Assert.Single(
                endpoint.GetMethods(BindingFlags.Instance | BindingFlags.Public),
                candidate => candidate.Name == "HandleAsync" && candidate.DeclaringType == endpoint
            );
            Assert.NotNull(method.GetCustomAttribute<AuthorizeByAttribute>());
        }
    }

    [Fact]
    public void VerifiedPayOsWebhook_RemainsPublicAndBypassMarkerIsNotClientSettable()
    {
        MethodInfo method = Assert.Single(
            typeof(CompletedOrderWebhook).GetMethods(BindingFlags.Instance | BindingFlags.Public),
            candidate =>
                candidate.Name == "HandleAsync"
                && candidate.DeclaringType == typeof(CompletedOrderWebhook)
        );
        PropertyInfo amountProperty = Assert.IsAssignableFrom<PropertyInfo>(
            typeof(UpdateStatusCommand).GetProperty(nameof(UpdateStatusCommand.ExpectedPaymentAmount))
        );
        PropertyInfo markerProperty = Assert.IsAssignableFrom<PropertyInfo>(
            typeof(UpdateStatusCommand).GetProperty(
                "IsVerifiedPayOsWebhook",
                BindingFlags.Instance | BindingFlags.NonPublic
            )
        );

        Assert.Null(method.GetCustomAttribute<AuthorizeByAttribute>());
        Assert.False(amountProperty.SetMethod?.IsPublic);
        Assert.False(markerProperty.SetMethod?.IsPublic);
    }

    private static CreateOrderHandler CreateHandler(
        IUnitOfWork unitOfWork,
        ICurrentAccount currentAccount
    ) =>
        new(
            unitOfWork,
            currentAccount,
            new OrgSetting(),
            Mock.Of<IEncryptionService>(),
            Mock.Of<IQrGenerator>()
        );

    private static (Mock<IUnitOfWork>, Mock<IDynamicSpecificationRepository<Order>>)
        BranchLookupUnitOfWork(long branchId)
    {
        Mock<IAsyncRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(x =>
                x.FindByConditionAsync(
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Expression<Func<Order, OrderBranchReference>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new OrderBranchReference(branchId));
        Mock<IDynamicSpecificationRepository<Order>> details = new(MockBehavior.Strict);
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork.Setup(x => x.Repository<Order>(false)).Returns(orders.Object);
        unitOfWork
            .Setup(x => x.DynamicReadOnlyRepository<Order>(false))
            .Returns(details.Object);
        return (unitOfWork, details);
    }

    private static (Mock<IUnitOfWork>, Mock<IDynamicSpecificationRepository<Order>>)
        TransactionalOrderUnitOfWork(Order order, bool commitExpected = false)
    {
        Mock<DbTransaction> transaction = new();
        Mock<IDynamicSpecificationRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(x =>
                x.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(order);
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction.Object);
        unitOfWork
            .Setup(x => x.DynamicReadOnlyRepository<Order>(false))
            .Returns(orders.Object);
        if (commitExpected)
            unitOfWork
                .Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        else
            unitOfWork
                .Setup(x => x.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        return (unitOfWork, orders);
    }

    private static Mock<IUnitOfWork> PaymentOrderUnitOfWork(long branchId)
    {
        Mock<IDynamicSpecificationRepository<Order>> orders = new(MockBehavior.Strict);
        orders
            .Setup(x =>
                x.FindByConditionAsync(
                    It.IsAny<ISpecification<Order>>(),
                    It.IsAny<Expression<Func<Order, OrderPayment>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new OrderPayment
                {
                    Id = branchId == 1 ? 10 : 20,
                    BranchId = branchId,
                    Code = "ORD-1",
                    Amount = 100,
                    Status = OrderStatus.Processed,
                    Items =
                    [
                        new OrderPaymentItem { Name = "Wash", Quantity = 1, Amount = 100 },
                    ],
                }
            );
        Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
        unitOfWork
            .Setup(x => x.DynamicReadOnlyRepository<Order>(false))
            .Returns(orders.Object);
        return unitOfWork;
    }

    private static Order OrderAtBranch(long branchId, OrderStatus status) =>
        new(branchId, 1, $"ORD-{branchId}", 100, 100, status);

    private static ICurrentAccount CurrentAccount(IEnumerable<string> branches) =>
        new StubCurrentAccount
        {
            Id = 99,
            Session = new UserAuth { Id = 99, Role = "STAFF", Branches = branches },
        };

    private static CreatePaymentResult PaymentResult() =>
        new(
            bin: string.Empty,
            accountNumber: string.Empty,
            amount: 100,
            description: "ORD-1",
            orderCode: 10,
            currency: "VND",
            paymentLinkId: "payment-1",
            status: "PENDING",
            expiredAt: null,
            checkoutUrl: "https://pay.test",
            qrCode: string.Empty
        );

    private static void AssertForbidden(ErrorDetails? error)
    {
        Assert.IsType<ForbiddenError>(error);
        Assert.Equal(403, error.Status);
    }

    private sealed class StubCurrentAccount : ICurrentAccount
    {
        public long? Id { get; init; }
        public string? ClientIp { get; private set; }
        public UserAuth? Session { get; init; }

        public Task SetClaimPrinciple(System.Security.Claims.ClaimsPrincipal user) =>
            Task.CompletedTask;

        public void SetClientIp(Microsoft.AspNetCore.Http.HttpContext httpContext) =>
            ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
