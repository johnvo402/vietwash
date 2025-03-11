using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Tariffs
{
    public class ServiceTariff
    {
        public Ulid TariffId { get; set; } = default!;
        public Ulid ServiceId = default!;
        public Ulid UnitRelationId = default!;
        public long Price { get; set; } = default!;
    }
}
