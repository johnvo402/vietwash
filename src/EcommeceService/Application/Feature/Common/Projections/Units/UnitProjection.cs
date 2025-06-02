using Domain.Aggregates.Services.Enums;
using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
        public ActivationStatus Status { get; set; } 
    }
}
