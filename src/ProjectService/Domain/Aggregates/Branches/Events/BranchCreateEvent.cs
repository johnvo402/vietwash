using Mediator;

namespace Domain.Aggregates.Branches.Events
{
    public class BranchCreateEvent : INotification
    {
        public long BranchId { get; set; }
        public string Name { get; set; } = default!;
    }
}
