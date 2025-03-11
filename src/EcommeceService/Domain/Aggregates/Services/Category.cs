using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Services
{
    public class Category : AggregateRoot
    {

        string Name { get; set; } = default!;
        public virtual ICollection<Service> Services { get; set; } = [];
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
