using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Tariffs
{
    public class ServiceTariff : DefaultEntity
    {
        public Ulid TariffId { get; set; } = default!;
        public Ulid ServiceId = default!;
        public Ulid UnitRelationId = default!;

        public Tariff Tariff { get; set; } = default!;
        public Service Service { get; set; } = default!;
        public UnitRelation UnitRelation { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }
}
