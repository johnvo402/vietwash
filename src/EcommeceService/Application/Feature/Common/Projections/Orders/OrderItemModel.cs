namespace Application.Feature.Common.Projections.Orders
{
    public class OrderItemModel
    {
        public long ServiceId { get; set; }
        public long UnitRelationId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? UnitRelationName { get; set; }
        public decimal ProcessingTime { get; set; }
        public string? ServiceName { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderEquipmentModel
    {
        public long EquipmentId { get; set; }

        public string EquipmentName { get; set; } = default!;
    }
}
