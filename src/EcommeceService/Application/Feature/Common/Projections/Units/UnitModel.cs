using Domain.Aggregates.Services.Enums;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitModel
    {
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } = ActivationStatus.active;
    }
}
