using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.UpdateStatus;

public static class EquipmentSelectionPolicy
{
    public static EquipmentLifecycleAction GetLifecycleAction(
        OrderStatus current,
        OrderStatus target
    ) =>
        (current, target) switch
        {
            (OrderStatus.Pending, OrderStatus.InProgress) => EquipmentLifecycleAction.Claim,
            (OrderStatus.InProgress, OrderStatus.Processed) => EquipmentLifecycleAction.Release,
            (OrderStatus.InProgress, OrderStatus.Cancelled) => EquipmentLifecycleAction.Release,
            _ => EquipmentLifecycleAction.None,
        };

    public static EquipmentSelectionResult Resolve(
        long branchId,
        IReadOnlyCollection<long> requestedIds,
        IReadOnlyCollection<EquipmentSnapshot> candidates
    )
    {
        if (requestedIds.Count == 0)
            return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.Empty);
        if (requestedIds.Any(id => id <= 0))
            return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.InvalidId);
        if (requestedIds.Distinct().Count() != requestedIds.Count)
            return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.Duplicate);

        Dictionary<long, EquipmentSnapshot> byId = candidates.ToDictionary(x => x.Id);
        List<OrderEquipment> resolved = [];
        foreach (long id in requestedIds)
        {
            if (!byId.TryGetValue(id, out EquipmentSnapshot? equipment))
                return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.NotFound);
            if (equipment.BranchId != branchId)
                return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.WrongBranch);
            if (equipment.Status != EquipmentStatus.Active)
                return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.Unavailable);
            if (equipment.Using)
                return EquipmentSelectionResult.Failure(EquipmentSelectionFailure.InUse);

            resolved.Add(
                new OrderEquipment { EquipmentId = equipment.Id, EquipmentName = equipment.Name }
            );
        }

        return EquipmentSelectionResult.Success(resolved);
    }
}

public sealed record EquipmentSnapshot(
    long Id,
    string Name,
    long BranchId,
    EquipmentStatus Status,
    bool Using
);

public sealed record EquipmentSelectionResult(
    bool IsSuccess,
    EquipmentSelectionFailure FailureReason,
    IReadOnlyList<OrderEquipment> Equipments
)
{
    public static EquipmentSelectionResult Success(IReadOnlyList<OrderEquipment> equipments) =>
        new(true, EquipmentSelectionFailure.None, equipments);

    public static EquipmentSelectionResult Failure(EquipmentSelectionFailure reason) =>
        new(false, reason, []);
}

public enum EquipmentSelectionFailure
{
    None,
    Empty,
    InvalidId,
    Duplicate,
    NotFound,
    WrongBranch,
    Unavailable,
    InUse,
}

public enum EquipmentLifecycleAction
{
    None,
    Claim,
    Release,
}
