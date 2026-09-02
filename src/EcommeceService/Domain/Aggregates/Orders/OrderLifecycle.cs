using Domain.Aggregates.Orders.Enums;

namespace Domain.Aggregates.Orders;

public static class OrderLifecycle
{
    public static bool CanTransition(OrderStatus current, OrderStatus target) =>
        current == target
        || (current, target)
            is
                (OrderStatus.Pending, OrderStatus.InProgress)
                or
                (OrderStatus.Pending, OrderStatus.Cancelled)
                or
                (OrderStatus.InProgress, OrderStatus.Processed)
                or
                (OrderStatus.InProgress, OrderStatus.Cancelled)
                or
                (OrderStatus.Processed, OrderStatus.Completed)
                or
                (OrderStatus.Processed, OrderStatus.Cancelled);

    public static bool CanEditDetails(OrderStatus status) => status == OrderStatus.Pending;
}

public enum OrderTransitionResult
{
    Applied,
    Idempotent,
    InvalidTransition,
    PaymentMethodRequired,
    PaymentMethodNotAllowed,
    EquipmentRequired,
    EquipmentNotAllowed,
    CancellationRequired,
    CancellationNotAllowed,
}
