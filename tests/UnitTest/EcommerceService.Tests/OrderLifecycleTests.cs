using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Events;
using Domain.Aggregates.Vouchers;
using Domain.Aggregates.Vouchers.Events;
using Domain.Events;

namespace EcommerceService.Tests;

public class OrderLifecycleTests
{
    private static readonly DateTimeOffset CompletedAt = new(
        2026,
        9,
        1,
        12,
        0,
        0,
        TimeSpan.Zero
    );

    public static IEnumerable<object[]> StatusPairs()
    {
        foreach (OrderStatus current in Enum.GetValues<OrderStatus>())
        foreach (OrderStatus target in Enum.GetValues<OrderStatus>())
            yield return [current, target, ExpectedTransition(current, target)];
    }

    [Theory]
    [MemberData(nameof(StatusPairs))]
    public void StateMachine_AllowsOnlyExplicitTransitions(
        OrderStatus current,
        OrderStatus target,
        bool expected
    ) => Assert.Equal(expected, OrderLifecycle.CanTransition(current, target));

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Processed)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Processed, OrderStatus.Completed)]
    [InlineData(OrderStatus.Processed, OrderStatus.Cancelled)]
    public void Domain_AppliesEveryAllowedTransition(OrderStatus current, OrderStatus target)
    {
        Order order = CreateOrder(current);
        IReadOnlyCollection<OrderEquipment>? equipments = target == OrderStatus.InProgress
            ? [new OrderEquipment { EquipmentId = 1, EquipmentName = "Washer" }]
            : null;
        PaymentMethod? payment = target == OrderStatus.Completed ? PaymentMethod.Cash : null;

        Assert.Equal(OrderTransitionResult.Applied, order.TransitionTo(target, payment, equipments));
        Assert.Equal(target, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.Processed)]
    [InlineData(OrderStatus.Pending, OrderStatus.Completed)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Completed)]
    [InlineData(OrderStatus.Completed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.InProgress)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Processed)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Completed)]
    public void Domain_RejectsSkippedAndTerminalTransitions(
        OrderStatus current,
        OrderStatus target
    )
    {
        Order order = CreateOrder(current);

        Assert.Equal(
            OrderTransitionResult.InvalidTransition,
            order.TransitionTo(target, PaymentMethod.Cash)
        );
        Assert.Equal(current, order.Status);
        Assert.Empty(order.UncommittedEvents);
    }

    [Theory]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Cancelled)]
    public void TerminalSameState_IsIdempotent(OrderStatus status)
    {
        Order order = CreateOrder(status);

        Assert.Equal(OrderTransitionResult.Idempotent, order.TransitionTo(status));
        Assert.Empty(order.UncommittedEvents);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, true)]
    [InlineData(OrderStatus.InProgress, false)]
    [InlineData(OrderStatus.Processed, false)]
    [InlineData(OrderStatus.Completed, false)]
    [InlineData(OrderStatus.Cancelled, false)]
    public void OrderDetails_AreEditableOnlyWhilePending(OrderStatus status, bool expected) =>
        Assert.Equal(expected, OrderLifecycle.CanEditDetails(status));

    [Fact]
    public void StartingOrder_RequiresEquipment()
    {
        Order order = CreateOrder(OrderStatus.Pending);

        Assert.Equal(
            OrderTransitionResult.EquipmentRequired,
            order.TransitionTo(OrderStatus.InProgress)
        );
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.UncommittedEvents);
    }

    [Fact]
    public void StartingOrder_PersistsAuthoritativeEquipmentSnapshot()
    {
        Order order = CreateOrder(OrderStatus.Pending);
        var equipment = new OrderEquipment
        {
            EquipmentId = 42,
            EquipmentName = "DB Washer Name",
        };

        Assert.Equal(
            OrderTransitionResult.Applied,
            order.TransitionTo(OrderStatus.InProgress, orderEquipments: [equipment])
        );
        Assert.Same(equipment, order.OrderEquipments.Single());
        Assert.Equal("DB Washer Name", order.OrderEquipments.Single().EquipmentName);
    }

    [Fact]
    public void CompletingOrder_RequiresPaymentMethodBeforeAnyMutation()
    {
        Order order = CreateOrder(OrderStatus.Processed, customerId: 7, voucherId: 8);

        Assert.Equal(
            OrderTransitionResult.PaymentMethodRequired,
            order.TransitionTo(OrderStatus.Completed)
        );
        Assert.Equal(OrderStatus.Processed, order.Status);
        Assert.Null(order.PaymentMethod);
        Assert.Empty(order.UncommittedEvents);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash)]
    [InlineData(PaymentMethod.Card)]
    public void CompletingOrder_AppliesPaymentBeforeEmittingSideEffects(PaymentMethod method)
    {
        Order order = CreateOrder(OrderStatus.Processed);

        Assert.Equal(
            OrderTransitionResult.Applied,
            order.TransitionTo(OrderStatus.Completed, method, transitionedAt: CompletedAt)
        );

        Assert.Equal(method, order.PaymentMethod);
        Assert.Equal(CompletedAt, order.OrderDate);
        Assert.Equal(
            method,
            Assert.Single(order.UncommittedEvents.OfType<CreateFundEvent>()).PaymentMethod
        );
        Assert.Single(order.UncommittedEvents.OfType<EInvoiceEvent>());
        Assert.Single(order.UncommittedEvents.OfType<UpdateStatusOrderEvent>());
    }

    [Fact]
    public void DuplicateCompletion_IsIdempotentAndDoesNotEmitSideEffectsTwice()
    {
        Order order = CreateOrder(OrderStatus.Processed, customerId: 7, voucherId: 8);
        _ = order.TransitionTo(OrderStatus.Completed, PaymentMethod.Card);
        int eventCount = order.UncommittedEvents.Count;

        Assert.Equal(
            OrderTransitionResult.Idempotent,
            order.TransitionTo(OrderStatus.Completed, PaymentMethod.Card)
        );
        Assert.Equal(eventCount, order.UncommittedEvents.Count);
        Assert.Single(order.UncommittedEvents.OfType<CreateFundEvent>());
        Assert.Single(order.UncommittedEvents.OfType<EInvoiceEvent>());
        Assert.Single(order.UncommittedEvents.OfType<VoucherUsageEvent>());
    }

    [Fact]
    public void Completion_EmitsOneVoucherUsageForVoucherAndCustomer()
    {
        Order order = CreateOrder(OrderStatus.Processed, customerId: 7, voucherId: 8);

        _ = order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash);

        VoucherUsageEvent usage = Assert.Single(
            order.UncommittedEvents.OfType<VoucherUsageEvent>()
        );
        Assert.Equal(order.Id, usage.OrderId);
        Assert.Equal(7, usage.CustomerId);
        Assert.Equal(8, usage.VoucherId);
        Assert.Equal(0, usage.DiscountApply);
    }

    [Theory]
    [InlineData(null, 8L)]
    [InlineData(7L, null)]
    [InlineData(null, null)]
    public void Completion_DoesNotEmitVoucherUsageWithoutBothReferences(
        long? customerId,
        long? voucherId
    )
    {
        Order order = CreateOrder(OrderStatus.Processed, customerId, voucherId);

        _ = order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash);

        Assert.Empty(order.UncommittedEvents.OfType<VoucherUsageEvent>());
    }

    [Fact]
    public void NonCompletionTransition_RejectsPaymentMethod()
    {
        Order order = CreateOrder(OrderStatus.Pending);

        Assert.Equal(
            OrderTransitionResult.PaymentMethodNotAllowed,
            order.TransitionTo(
                OrderStatus.InProgress,
                PaymentMethod.Cash,
                [new OrderEquipment { EquipmentId = 1, EquipmentName = "Washer" }]
            )
        );
    }

    [Fact]
    public void InvalidTransition_DoesNotMutateOrEmitEvents()
    {
        Order order = CreateOrder(OrderStatus.Pending);

        Assert.Equal(
            OrderTransitionResult.InvalidTransition,
            order.TransitionTo(OrderStatus.Completed, PaymentMethod.Cash)
        );
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Empty(order.UncommittedEvents);
    }

    [Fact]
    public void EquipmentSelection_UsesDatabaseNameAndAcceptsOnlyAvailableBranchEquipment()
    {
        EquipmentSelectionResult result = EquipmentSelectionPolicy.Resolve(
            10,
            [1],
            [new EquipmentSnapshot(1, "Authoritative", 10, EquipmentStatus.Active, false)]
        );

        Assert.True(result.IsSuccess);
        Assert.Equal("Authoritative", result.Equipments.Single().EquipmentName);
    }

    [Theory]
    [InlineData(EquipmentStatus.Active, true, 10, EquipmentSelectionFailure.InUse)]
    [InlineData(EquipmentStatus.UnderMaintenance, false, 10, EquipmentSelectionFailure.Unavailable)]
    [InlineData(EquipmentStatus.UnderRepair, false, 10, EquipmentSelectionFailure.Unavailable)]
    [InlineData(EquipmentStatus.Active, false, 99, EquipmentSelectionFailure.WrongBranch)]
    public void EquipmentSelection_RejectsUnavailableEquipment(
        EquipmentStatus status,
        bool usingEquipment,
        long branchId,
        EquipmentSelectionFailure expected
    )
    {
        EquipmentSelectionResult result = EquipmentSelectionPolicy.Resolve(
            10,
            [1],
            [new EquipmentSnapshot(1, "Washer", branchId, status, usingEquipment)]
        );

        Assert.False(result.IsSuccess);
        Assert.Equal(expected, result.FailureReason);
    }

    [Fact]
    public void EquipmentSelection_RejectsDuplicateAndMissingIds()
    {
        EquipmentSnapshot equipment = new(1, "Washer", 10, EquipmentStatus.Active, false);

        Assert.Equal(
            EquipmentSelectionFailure.Duplicate,
            EquipmentSelectionPolicy.Resolve(10, [1, 1], [equipment]).FailureReason
        );
        Assert.Equal(
            EquipmentSelectionFailure.NotFound,
            EquipmentSelectionPolicy.Resolve(10, [1, 2], [equipment]).FailureReason
        );
    }

    [Theory]
    [InlineData(OrderStatus.Pending, OrderStatus.InProgress, EquipmentLifecycleAction.Claim)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Processed, EquipmentLifecycleAction.Release)]
    [InlineData(OrderStatus.InProgress, OrderStatus.Cancelled, EquipmentLifecycleAction.Release)]
    [InlineData(OrderStatus.Pending, OrderStatus.Cancelled, EquipmentLifecycleAction.None)]
    [InlineData(OrderStatus.Processed, OrderStatus.Cancelled, EquipmentLifecycleAction.None)]
    public void EquipmentLifecycle_ClaimsAndReleasesAtTheExpectedTransitions(
        OrderStatus current,
        OrderStatus target,
        EquipmentLifecycleAction expected
    ) => Assert.Equal(expected, EquipmentSelectionPolicy.GetLifecycleAction(current, target));

    [Fact]
    public void UpdateStatusContract_AcceptsEquipmentIdsOnly()
    {
        Assert.Null(typeof(OrderEquipmentSelectionModel).GetProperty("EquipmentName"));
        Assert.NotNull(typeof(OrderEquipmentSelectionModel).GetProperty("EquipmentId"));
    }

    [Fact]
    public void UpdateStatusValidator_RejectsMalformedOrderIdAndNullModel()
    {
        var validator = new UpdateStatusCommandValidator();

        Assert.False(
            validator
                .Validate(new UpdateStatusCommand { OrderId = "not-a-number", Model = null! })
                .IsValid
        );
    }

    [Fact]
    public void UpdateStatusValidator_EnforcesPaymentAndUniqueEquipmentIds()
    {
        var validator = new UpdateStatusCommandValidator();
        var missingPayment = new UpdateStatusCommand
        {
            OrderId = "1",
            Model = new OrderUpdateStatus { Status = OrderStatus.Completed },
        };
        var unexpectedPayment = new UpdateStatusCommand
        {
            OrderId = "1",
            Model = new OrderUpdateStatus
            {
                Status = OrderStatus.Processed,
                PaymentMethod = PaymentMethod.Cash,
            },
        };
        var duplicateEquipment = new UpdateStatusCommand
        {
            OrderId = "1",
            Model = new OrderUpdateStatus
            {
                Status = OrderStatus.InProgress,
                OrderEquipments =
                [
                    new() { EquipmentId = 5 },
                    new() { EquipmentId = 5 },
                ],
            },
        };

        Assert.False(validator.Validate(missingPayment).IsValid);
        Assert.False(validator.Validate(unexpectedPayment).IsValid);
        Assert.False(validator.Validate(duplicateEquipment).IsValid);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(123456789, 123456789)]
    public void PayOsCorrelation_UsesOrderId(long orderId, long expected) =>
        Assert.Equal(expected, PayOsOrderPolicy.GetOrderCode(orderId));

    [Fact]
    public void PayOsRetry_ReusesTheExistingPaymentReference()
    {
        Assert.Equal(
            "https://pay.payos.vn/web/payment-link-id",
            PayOsOrderPolicy.GetCheckoutUrl("payment-link-id")
        );
    }

    [Theory]
    [InlineData(100, true, 100)]
    [InlineData(0, false, 0)]
    [InlineData(-1, false, 0)]
    [InlineData(100.5, false, 0)]
    [InlineData(2147483648, false, 0)]
    public void PayOsAmount_RequiresPositiveWholeVnd(
        decimal total,
        bool expectedSuccess,
        int expectedAmount
    )
    {
        bool success = PayOsOrderPolicy.TryGetAmount(total, out int amount);

        Assert.Equal(expectedSuccess, success);
        Assert.Equal(expectedAmount, amount);
    }

    [Fact]
    public void VoucherUsage_AllowsZeroAppliedDiscount()
    {
        var usage = new VoucherUsage(1, 2, 3, 0);

        Assert.Equal(0, usage.DiscountApply);
    }

    private static bool ExpectedTransition(OrderStatus current, OrderStatus target) =>
        current == target
        || (current, target)
            is (OrderStatus.Pending, OrderStatus.InProgress)
                or (OrderStatus.Pending, OrderStatus.Cancelled)
                or (OrderStatus.InProgress, OrderStatus.Processed)
                or (OrderStatus.InProgress, OrderStatus.Cancelled)
                or (OrderStatus.Processed, OrderStatus.Completed)
                or (OrderStatus.Processed, OrderStatus.Cancelled);

    private static Order CreateOrder(
        OrderStatus status,
        long? customerId = null,
        long? voucherId = null
    ) =>
        new(
            branchId: 10,
            staffId: 20,
            code: "OD000001",
            amount: 100,
            total: 100,
            status: status,
            customerId: customerId,
            voucherId: voucherId
        );
}
