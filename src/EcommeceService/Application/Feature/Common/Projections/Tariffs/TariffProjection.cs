using Domain.Aggregates.Tariffs;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Tariffs
{
    public class TariffProjection : BaseEntity
    {
        public string Name { get; set; }
        public bool Disable { get; set; }

        public virtual void MappingFrom(Tariff tariff)
        {
            Id = tariff.Id;
            Name = tariff.Name;
            Disable = tariff.Disable;
        }
    }
}
