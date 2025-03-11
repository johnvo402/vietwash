using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Tariffs
{
    public class Tariff : AggregateRoot
    {
        public string Name { get; set; } = default!;
        public bool Disable { get; set; } = default!;
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
