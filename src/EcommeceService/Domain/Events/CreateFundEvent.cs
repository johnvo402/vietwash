using Domain.Aggregates.Orders.Enums;
using Domain.Events.Enums;
using Mediator;
using Shared.Kernel.Common.Events;

namespace Domain.Events
{
    public class CreateFundEvent : INotification, IIntegrationEvent
    {
        public Guid MessageId { get; set; }
        public string TypeId { get; set; } = default!;
        public long BehaviorId { get; set; } = default!;
        public long ReferenceId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public Dictionary<string, object>? Metadata { get; set; }
        public long BranchId { get; set; } = default!;
        public long? ObjectId { get; set; }
        public decimal Point { get; set; }
        public DateTimeOffset TransactionAt { get; set; }
        public FundEventType FundEventType { get; set; }
    }
}
