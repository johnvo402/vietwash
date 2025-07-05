using Mediator;

namespace Domain.Aggregates.Products.Events
{
    public class BranchProductCreateEvent : INotification
    {
        public BranchProduct BranchProduct { get; set; } = default!;
    }
}
