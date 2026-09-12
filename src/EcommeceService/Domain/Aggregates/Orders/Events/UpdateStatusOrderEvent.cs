using Mediator;
using Shared.Kernel.Common.Events;

namespace Domain.Aggregates.Orders.Events
{
    public class UpdateStatusOrderEvent : IDomainEvent
    {
        public Order Order { get; set; } = default!;
    }
}
