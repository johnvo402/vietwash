using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.UpdateStatus;

public static class OrderCancellationResourcePolicy
{
    public static OrderCancellationResourcePlan Create(
        OrderStatus previousStatus,
        OrderStatus target,
        long? customerId,
        long? voucherId
    )
    {
        bool isCancellation = target == OrderStatus.Cancelled;
        return new OrderCancellationResourcePlan(
            IsCancellation: isCancellation,
            ShouldReleaseVoucher: isCancellation && customerId.HasValue && voucherId.HasValue,
            RequiresPayOsCoordination: isCancellation && previousStatus == OrderStatus.Processed,
            MaterialsRemainConsumed: isCancellation
        );
    }
}

public sealed record OrderCancellationResourcePlan(
    bool IsCancellation,
    bool ShouldReleaseVoucher,
    bool RequiresPayOsCoordination,
    bool MaterialsRemainConsumed
);
