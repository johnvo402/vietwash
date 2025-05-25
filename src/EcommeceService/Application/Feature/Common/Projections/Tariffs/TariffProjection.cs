using JohnChum.SharedKernel.Domain.Common;

namespace Application.Feature.Common.Projections.Tariffs
{
    public class TariffProjection : BaseEntity
    {
        public string Name { get; set; }
        public bool Disable { get; set; }
    }
}