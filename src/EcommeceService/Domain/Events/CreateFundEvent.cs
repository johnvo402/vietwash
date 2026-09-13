using Domain.Aggregates.Orders.Enums;
using Domain.Events.Enums;
using Shared.Kernel.Common.Events;

namespace Domain.Events
{
    public sealed record CreateFundEvent : IDomainEvent, IIntegrationEvent
    {
        public Guid MessageId { get; init; }
        public string TypeId { get; init; } = default!;
        public long BehaviorId { get; init; }
        public long ReferenceId { get; init; }
        public decimal Amount { get; init; }
        public PaymentMethod PaymentMethod { get; init; }
        public IReadOnlyDictionary<string, object>? Metadata { get; init; }
        public long BranchId { get; init; }
        public long? ObjectId { get; init; }
        public decimal Point { get; init; }
        public DateTimeOffset TransactionAt { get; init; }
        public FundEventType FundEventType { get; init; }
    }
}
