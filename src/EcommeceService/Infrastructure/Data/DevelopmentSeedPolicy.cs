using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Infrastructure.Data;

/// <summary>Development fixtures only. Never invoked by runtime order or inventory commands.</summary>
public static class DevelopmentSeedPolicy
{
    public static readonly long[] BranchIds = [1, 2, 3];
    public static readonly string[] ProductNames = ["Bột giặt Omo", "Nước xả Downy", "Chất tẩy Javel", "Găng tay cao su"];
    public const string OrderNote = "VietWash development seed order";

    public static bool IsSeedImport(InventoryDocument document) =>
        document.Type == InventoryType.Import
        && document.Status == InventoryStatus.Completed
        && (document.Code.StartsWith("DEV-IM-", StringComparison.Ordinal)
            || (document.Code.StartsWith("IM", StringComparison.Ordinal)
                && document.TransactionAt.HasValue
                && document.Note == $"Phiếu nhập hàng tháng {document.TransactionAt:MM/yyyy}"));

    public static IReadOnlyList<Equipment> MissingEquipment(
        IEnumerable<InventoryDocument> documents,
        IEnumerable<Equipment> existing,
        DateTimeOffset maintenanceAt
    )
    {
        // The schema has a non-unique citext Code index, not a global unique key.
        // Reconcile a receipt's branch/code case-insensitively without changing database constraints.
        var identities = existing.Select(x => (x.BranchId, Code: x.Code.ToUpperInvariant())).ToHashSet();
        var missing = new List<Equipment>();
        foreach (InventoryDocument document in documents.Where(IsSeedImport))
        {
            if (!document.BranchId.HasValue || !BranchIds.Contains(document.BranchId.Value))
                throw new InvalidOperationException($"Seed inventory {document.Code} has missing/invalid BranchId {document.BranchId}.");
            if (document.EquipmentSupplyings.Any(x => x.Quantity <= 0 || string.IsNullOrWhiteSpace(x.Code)))
                throw new InvalidOperationException($"Seed inventory {document.Code} contains invalid equipment quantity/code.");
            foreach (Equipment equipment in InventoryEquipmentFactory.Create(document, maintenanceAt))
                if (identities.Add((equipment.BranchId, equipment.Code.ToUpperInvariant())))
                    missing.Add(equipment);
        }
        return missing;
    }

    public static IReadOnlyList<Equipment> SelectEquipment(
        long branchId,
        OrderStatus status,
        IEnumerable<Equipment> equipments,
        ISet<long> reservedIds,
        Random random
    )
    {
        if (status is OrderStatus.Pending or OrderStatus.Cancelled)
            return [];
        var candidates = equipments.Where(x => x.BranchId == branchId && x.Status == EquipmentStatus.Active)
            .Where(x => status != OrderStatus.InProgress || (!x.Using && !reservedIds.Contains(x.Id)))
            .OrderBy(x => x.Id).ToArray();
        if (candidates.Length == 0)
            throw new InvalidOperationException($"No active seed equipment exists for Branch {branchId} (available for {status}).");
        Equipment selected = candidates[random.Next(candidates.Length)];
        if (status == OrderStatus.InProgress)
        {
            reservedIds.Add(selected.Id);
            selected.Using = true;
        }
        return [selected];
    }

    public static void ValidateOrders(IEnumerable<Order> orders, IEnumerable<Equipment> equipments)
    {
        var byId = equipments.ToDictionary(x => x.Id);
        var activeClaims = new HashSet<long>();
        foreach (Order order in orders)
        {
            if (order.Status == OrderStatus.InProgress && order.OrderEquipments.Count == 0)
                throw new InvalidOperationException($"Seed order {order.Code} is InProgress without equipment.");
            if (order.Status == OrderStatus.Completed && order.PaymentMethod != PaymentMethod.Cash)
                throw new InvalidOperationException($"Seed order {order.Code} must be completed with Cash.");
            foreach (OrderEquipment link in order.OrderEquipments)
            {
                if (!byId.TryGetValue(link.EquipmentId, out Equipment? equipment) || equipment.BranchId != order.BranchId)
                    throw new InvalidOperationException($"Seed order {order.Code} has missing or cross-branch equipment {link.EquipmentId}.");
                if (order.Status == OrderStatus.InProgress && (!equipment.Using || !activeClaims.Add(equipment.Id)))
                    throw new InvalidOperationException($"Seed equipment {equipment.Code} is unclaimed or shared by multiple InProgress orders.");
            }
        }
    }
}
