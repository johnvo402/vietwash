using Domain.Aggregates.Enums;
using Domain.Aggregates.Services.Enums;

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
    }
}
