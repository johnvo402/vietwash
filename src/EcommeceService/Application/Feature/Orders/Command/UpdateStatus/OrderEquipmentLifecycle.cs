using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.UpdateStatus;

internal static class OrderEquipmentLifecycle
{
    public static async Task<EquipmentSelectionResult> ResolveAsync(
        IUnitOfWork unitOfWork,
        Order order,
        EquipmentLifecycleAction action,
        long[] requestedEquipmentIds,
        CancellationToken cancellationToken
    )
    {
        if (action != EquipmentLifecycleAction.Claim)
            return EquipmentSelectionResult.Success([]);

        List<EquipmentSnapshot> candidates = await unitOfWork
            .Repository<Equipment>()
            .QueryAsync(equipment => requestedEquipmentIds.Contains(equipment.Id))
            .Select(equipment =>
                new EquipmentSnapshot(
                    equipment.Id,
                    equipment.Name,
                    equipment.BranchId,
                    equipment.Status,
                    equipment.Using
                )
            )
            .ToListAsync(cancellationToken);
        return EquipmentSelectionPolicy.Resolve(
            order.BranchId,
            requestedEquipmentIds,
            candidates
        );
    }

    public static async Task<bool> ApplyAsync(
        IUnitOfWork unitOfWork,
        Order order,
        EquipmentLifecycleAction action,
        long[] requestedEquipmentIds,
        CancellationToken cancellationToken
    )
    {
        if (action == EquipmentLifecycleAction.Claim)
        {
            int claimedRows = await unitOfWork
                .Repository<Equipment>()
                .QueryAsync(equipment =>
                    requestedEquipmentIds.Contains(equipment.Id)
                    && equipment.BranchId == order.BranchId
                    && equipment.Status == EquipmentStatus.Active
                    && !equipment.Using
                )
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(equipment => equipment.Using, true),
                    cancellationToken
                );
            return claimedRows == requestedEquipmentIds.Length;
        }

        if (action == EquipmentLifecycleAction.Release)
        {
            long[] equipmentIds = order.OrderEquipments
                .Select(equipment => equipment.EquipmentId)
                .ToArray();
            if (equipmentIds.Length != 0)
                _ = await unitOfWork
                    .Repository<Equipment>()
                    .QueryAsync(equipment =>
                        equipmentIds.Contains(equipment.Id)
                        && equipment.BranchId == order.BranchId
                    )
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(equipment => equipment.Using, false),
                        cancellationToken
                    );
        }

        return true;
    }
}
