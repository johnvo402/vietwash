using Domain.Aggregates.Enums;
using Domain.Aggregates.Services.Enums;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } 
    }
}
