using Domain.Aggregates.Funds.Enums;
using Shared.Kernel.Common;
using Mediator;

namespace Domain.Aggregates.Funds
{
    public class Transaction : AggregateRoot
    {
        public long CustomerId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public DateTimeOffset TransactionAt { get; set; } = default!;
        public TransactionType Type { get; set; } = default!;

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
