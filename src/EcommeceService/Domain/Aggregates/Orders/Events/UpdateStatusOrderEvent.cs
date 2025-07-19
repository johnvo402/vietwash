using Mediator;

namespace Domain.Aggregates.Orders.Events
{
    public class UpdateStatusOrderEvent : INotification
    {
        public Order Order { get; set; }
    }
}
