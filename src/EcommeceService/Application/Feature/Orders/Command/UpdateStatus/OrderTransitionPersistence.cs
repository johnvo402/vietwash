using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.UpdateStatus;

internal static class OrderTransitionPersistence
{
    public static async Task<OrderTransitionWriteResult> ReserveAsync(
        IUnitOfWork unitOfWork,
        Order order,
        OrderStatus previousStatus,
        OrderStatus target,
        CancellationToken cancellationToken
    )
    {
        int transitionedRows = await unitOfWork
            .Repository<Order>()
            .QueryAsync(candidate =>
                candidate.Id == order.Id
                && candidate.Status == previousStatus
                && candidate.Version == order.Version
            )
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(candidate => candidate.Status, target),
                cancellationToken
            );
        if (transitionedRows == 1)
            return OrderTransitionWriteResult.Applied;

        OrderStatus? persistedStatus = await unitOfWork
            .Repository<Order>()
            .QueryAsync(candidate => candidate.Id == order.Id)
            .AsNoTracking()
            .Select(candidate => (OrderStatus?)candidate.Status)
            .SingleOrDefaultAsync(cancellationToken);
        return persistedStatus == target
            ? OrderTransitionWriteResult.AlreadyApplied
            : OrderTransitionWriteResult.Conflict;
    }
}

internal enum OrderTransitionWriteResult
{
    Applied,
    AlreadyApplied,
    Conflict,
}
