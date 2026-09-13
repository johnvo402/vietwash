using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Products.Events
{
    public class BranchProductCreateEvent : IDomainEvent
    {
        public BranchProduct BranchProduct { get; set; } = default!;
    }
}
