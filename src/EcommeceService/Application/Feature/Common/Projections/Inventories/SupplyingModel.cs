namespace Application.Feature.Common.Projections.Inventories
{
    public class ProductSupplyingModel
    {
        public long ProductId { get; set; } = default!;
        public long? SupplierId { get; set; }
        public decimal Quantity { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
    }

    public class EquipmentSupplyingModel
    {
        public string Name { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string? Image { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public long SupplierId { get; set; } = default!;
    }
}
