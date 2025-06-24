using Ardalis.GuardClauses;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Tariffs
{
    public class Tariff : AggregateRoot
    {
        public string Name { get; set; } = default!;
        public bool Disable { get; set; } = default!;
        public ICollection<ServiceTariff> ServiceTariffs { get; set; } = [];

        public ICollection<ServicePriceTariffHistory> ServicePriceTariffHistories { get; set; } =
            [];

        public long BranchId { get; set; } = default!;

        public Tariff(string name, long branchId, bool disable = false)
        {
            Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Guard.Against.NegativeOrZero(branchId, nameof(branchId));

            Name = name.Trim();
            BranchId = branchId;
            Disable = disable;
        }

        public void Update(string? name = null, bool? disable = null, long? branchId = null)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }
            if (branchId.HasValue)
            {
                BranchId = (long)branchId;
            }
            if (disable.HasValue)
            {
                Disable = (bool)disable;
            }
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
