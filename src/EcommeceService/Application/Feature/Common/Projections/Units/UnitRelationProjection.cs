using Domain.Aggregates.Enums;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitRelationProjection
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public bool BaseUnit { get; set; }
        public decimal Price { get; set; }
        public int Multiple { get; set; }
        public decimal ProcessingTime { get; set; }
        public ActivationStatus Status { get; set; }

        public ICollection<ServiceResourceProjection> ServiceResources { get; set; } = [];
    }

    public class ServiceResourceProjection
    {
        public long UnitProductId { get; set; }
        public long ProductId { get; set; }
        public string UnitName { get; set; }
        public string ProductName { get; set; }
        public decimal Quantity { get; set; }
    }
}
