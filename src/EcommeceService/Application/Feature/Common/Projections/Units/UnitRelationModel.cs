using Domain.Aggregates.Enums;
using Domain.Aggregates.Services;
namespace Application.Feature.Common.Projections.Units
{
    public class UnitRelationModel
    {
        public ActivationStatus Status { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool BaseUnit { get; set; } = default!;
        public decimal Price { get; set; } = default!;
        public int Multiple { get; set; } = 1; // Mặc định là 1 cho Service
        public decimal ProcessingTime { get; set; } = default!;

        public virtual void MappingFrom(UnitRelation unitRelation)
        {
            Name = unitRelation.Name;
            BaseUnit = unitRelation.BaseUnit;
            Price = unitRelation.Price;
            Multiple = unitRelation.Multiple;
            ProcessingTime = unitRelation.ProcessingTime;
            Status = unitRelation.Status;
        }
    }
}
