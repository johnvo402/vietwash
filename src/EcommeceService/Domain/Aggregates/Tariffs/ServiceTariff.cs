using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Tariffs
{
    public class ServiceTariff : BaseEntity<long>
    {
        public long TariffId { get; set; } = default!;
        public long ServiceId = default!;
        public long UnitRelationId = default!;

        public Tariff Tariff { get; set; } = default!;
        public Service Service { get; set; } = default!;
        public UnitRelation UnitRelation { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }
}
