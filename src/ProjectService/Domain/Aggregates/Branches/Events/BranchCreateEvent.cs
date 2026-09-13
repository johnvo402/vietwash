using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Branches.Events
{
    public class BranchCreateEvent : IDomainEvent
    {
        public long BranchId { get; set; }
        public string Name { get; set; } = default!;
    }
}
