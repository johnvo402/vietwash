using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;

namespace Domain.Aggregates.Inventories;

/// <summary>Maps an inventory receipt to equipment; persistence and operational side effects belong to callers.</summary>
public static class InventoryEquipmentFactory
{
    public static IReadOnlyList<Equipment> Create(
        InventoryDocument document,
        DateTimeOffset maintenanceAt,
        long? fallbackBranchId = null
    )
    {
        long branchId = document.BranchId ?? fallbackBranchId
            ?? throw new InvalidOperationException($"Inventory document {document.Code} has no BranchId.");
        var equipments = new List<Equipment>();
        foreach (EquipmentSupplying supplying in document.EquipmentSupplyings)
        {
            for (int index = 0; index < supplying.Quantity; index++)
            {
                equipments.Add(new Equipment(
                    branchId: branchId,
                    name: supplying.Name,
                    code: index == 0 ? supplying.Code : $"{supplying.Code}{index}",
                    price: supplying.Price,
                    status: EquipmentStatus.Active,
                    description: document.Code,
                    lastMaintenanceOrRepairDate: maintenanceAt,
                    nextMaintenanceDate: maintenanceAt.AddMonths(6)
                )
                { Image = supplying.Image });
            }
        }
        return equipments;
    }
}
