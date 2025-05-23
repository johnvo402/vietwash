using Domain.Aggregates.Funds.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Funds
{
    public class Fund : AggregateRoot
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public FundType Type { get; set; } = default!;
        public FundStatus Status { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public long ObjectId { get; set; }
        public long FundBehaviorId { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset TransactionDate { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public long ReferenceId { get; set; } = default!;
        public long BranchId { get; set; } = default!;
        public FundBehavior FundBehavior { get; set; } = default!;
        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
