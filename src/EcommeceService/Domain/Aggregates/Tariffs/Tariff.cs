using Ardalis.GuardClauses;
using Domain.Aggregates.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Tariffs
{
    public class Tariff : AggregateRoot
    {
        public long BranchId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public bool Disable { get; set; } = default!;
        public ActivationStatus Status { get; set; } = default!;
        public DateTimeOffset? StartAt { get; set; }
        public DateTimeOffset? EndAt { get; set; }
        public ICollection<ServiceTariff> ServiceTariffs { get; set; } = [];

        public ICollection<ServicePriceTariffHistory> ServicePriceTariffHistories { get; set; } =
            [];

        public Tariff(
            string name,
            long branchId,
            ActivationStatus status,
            bool disable = false,
            DateTimeOffset? startAt = null,
            DateTimeOffset? endAt = null
        )
        {
            Name = Guard.Against.NullOrWhiteSpace(name, nameof(name)).Trim();
            BranchId = Guard.Against.NegativeOrZero(branchId, nameof(branchId));
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));
            Disable = disable;
            StartAt = startAt;
            EndAt = endAt;
        }

        public void Update(
            string? name = null,
            bool? disable = null,
            long? branchId = null,
            ActivationStatus? status = null,
            DateTimeOffset? startAt = null,
            DateTimeOffset? endAt = null
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            if (branchId.HasValue)
                BranchId = (long)branchId;
            if (disable.HasValue)
                Disable = (bool)disable;
            if (status.HasValue)
                Status = status.Value;
            if (startAt.HasValue)
                StartAt = startAt;
            if (endAt.HasValue)
                EndAt = endAt;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
