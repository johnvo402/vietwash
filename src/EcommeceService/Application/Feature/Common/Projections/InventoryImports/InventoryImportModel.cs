
using Domain.Aggregates.Inventories.Enums;

namespace Application.Feature.Common.Projections.InventoryImports
{
    public class InventoryImportModel
    {
        public long BranchId { get; set; }
        public long? FromWarehouseId { get; set; }
        public long ToWarehouseId { get; set; }
        public decimal? PaidAmount { get; set; }
        public InventoryPaymentMethod PaymentMethod { get; set; }
        public string? Note { get; set; } = string.Empty;
        public DateTimeOffset? ArrivedAt { get; set; }
        public List<ProductImportItem> ProductItems { get; set; } = [];
        public List<EquipmentImportItem> EquipmentItems { get; set; } = [];
    }

}
