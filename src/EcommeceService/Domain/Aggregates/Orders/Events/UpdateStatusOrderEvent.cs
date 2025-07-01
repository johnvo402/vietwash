using Domain.Aggregates.Orders.Enums;
using Mediator;

namespace Domain.Aggregates.Orders.Events
{
    public class UpdateStatusOrderEvent : INotification
    {
        public string TypeId { get; set; } = default!;
        public long BehaviorId { get; set; } = default!;
        public long OrderId { get; set; } = default!;
        public Ulid PublicId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public string? Code { get; set; }
        public long BranchId { get; set; } = default!;
        public long? CustomerId { get; set; }
    }
}
