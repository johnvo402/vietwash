using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Units
{
    public class UnitProjection : BaseEntity
    {
        public string Name { get; set; } = default!;
    }
}
