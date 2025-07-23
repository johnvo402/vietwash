using Application.Feature.Common.Mapping.Tariffs;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Tariffs;
using Shared.Kernel.Common;

namespace Application.Feature.Common.Projections.Tariffs
{
    public class TariffProjection : BaseEntity
    {
        public string Name { get; set; }
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public ActivationStatus Status { get; set; }
        public List<ServiceTariffProjection> ServiceTariffs { get; set; } = [];

        public virtual void MappingFrom(Tariff tariff)
        {
            Id = tariff.Id;
            Name = tariff.Name;
            StartAt = tariff.StartAt;
            EndAt = tariff.EndAt;
            Status = tariff.Status;
            ServiceTariffs = tariff
                .ServiceTariffs.Select(x => x.ToServiceTariffProjectionResponse())
                .ToList();
        }
    }
}
