using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Inventories
{
    public class InventoryDocumentModel
    {
        public PaymentMethod? PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public long? BranchId { get; set; }
        public long? WarehouseId { get; set; }
        public InventoryType Type { get; set; }
        public string? Note { get; set; }

        public ICollection<ProductSupplyingModel> ProductSupplyings { get; set; } = [];
        public ICollection<EquipmentSupplyingModel> EquipmentSupplyings { get; set; } = [];
    }

    public class InventoryDocumentUpdateStatus
    {
        public InventoryStatus Status { get; set; }
        public string? CancelReason { get; set; }
    }
}
