using Domain.Aggregates.Funds.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Domain.Aggregates.Funds
{
    public class Fund : AggregateRoot
    {
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Type { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public long BehaviorId { get; set; }
        public long ObjectId { get; set; }
        public string Note { get; set; } = default!;
        public DateTimeOffset TransactionDate { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public long ReferenceId { get; set; } = default!;
        public long BranchId { get; set; } = default!;

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
