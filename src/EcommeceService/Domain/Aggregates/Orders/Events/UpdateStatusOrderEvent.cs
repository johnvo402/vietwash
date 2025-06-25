using Domain.Aggregates.Orders.Enums;
using Mediator;

namespace Domain.Aggregates.Orders.Events
{
    public class UpdateStatusOrderEvent : INotification
    {
        public string TypeId { get; set; } = default!;
        public string BehaviorId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public long? ReferenceId { get; set; }
    }
}
