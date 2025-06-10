

using Domain.Aggregates.Inventories.Enums;

namespace Application.Feature.Common.Projections.InventoryImports
{
    public class InventoryImportDetailProjection : InventoryImportProjection
    {
        public List<ProductSupplyingProjection> ProductSupplyings { get; set; } = [];
        public List<EquipmentSupplyingProjection> EquipmentSupplyings { get; set; } = [];
    }
    public class ProductSupplyingProjection
    {
        public long ProductId { get; set; }
        public long SupplierId { get; set; }
        public long InventoryDocumentId { get; set; }
        public long UnitRelationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string LotNumber { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = default!;
        public InventoryDocumentType Type { get; set; }
        public DateTimeOffset ExperyDate { get; set; }
        public DateTimeOffset ArriveAt { get; set; }
    }

    public class EquipmentSupplyingProjection
    {
        public long EquipmentId { get; set; }
        public long SupplierId { get; set; }
        public long InventoryDocumentId { get; set; }
        public long UnitRelationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = default!;
        public decimal Capacity { get; set; }
        public InventoryDocumentType Type { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public DateTimeOffset ArriveAt { get; set; }
    }

}
