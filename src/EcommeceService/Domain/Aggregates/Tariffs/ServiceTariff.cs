using Domain.Aggregates.Services;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Tariffs
{
    public class ServiceTariff : BaseEntity<long>
    {
        public long TariffId { get; set; } = default!;
        public long ServiceId { get; set; } = default!;
        public long UnitRelationId { get; set; } = default!;
        public Tariff Tariff { get; set; } = default!;
        public Service Service { get; set; } = default!;
        public UnitRelation UnitRelation { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }
}
