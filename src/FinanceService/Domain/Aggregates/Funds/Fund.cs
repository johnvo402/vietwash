using Ardalis.GuardClauses;
using Domain.Aggregates.Funds.Enums;
using Domain.Aggregates.Users;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Funds
{
    public class Fund : AggregateRoot
    {
        public string Code { get; private set; } = default!;
        public string? Name { get; set; }
        public FundType Type { get; private set; } = default!;
        public FundStatus Status { get; private set; } = default!;
        public decimal Amount { get; private set; } = default!;
        public long? ObjectId { get; set; }
        public long FundBehaviorId { get; private set; }
        public string? Note { get; set; }
        public DateTimeOffset? TransactionDate { get; set; }
        public PaymentMethod PaymentMethod { get; private set; } = default!;
        public long? ReferenceId { get; set; }
        public long BranchId { get; set; }
        public Guid? SourceEventId { get; private set; }
        public FundBehavior FundBehavior { get; set; } = default!; // navigation
        public User User { get; set; } = default!; // navigation
        public object? Metadata { get; set; }

        public Fund(
            string code,
            string? name,
            FundType type,
            FundStatus status,
            decimal amount,
            long? objectId,
            long fundBehaviorId,
            string? note,
            DateTimeOffset? transactionDate,
            PaymentMethod paymentMethod,
            long? referenceId,
            long branchId,
            object? metadata,
            Guid? sourceEventId = null
        )
        {
            Code = Guard.Against.NullOrWhiteSpace(code);
            Name = name;
            Type = Guard.Against.Null(type);
            Status = Guard.Against.Null(status);
            Amount = Guard.Against.Negative(amount);
            ObjectId = objectId;
            FundBehaviorId = fundBehaviorId;
            PaymentMethod = Guard.Against.Null(paymentMethod);
            ReferenceId = referenceId;
            BranchId = branchId;
            SourceEventId = sourceEventId;

            Note = note;
            TransactionDate = transactionDate;
            Metadata = metadata;
        }

        public void Update(
            string? note = null,
            FundStatus? status = null,
            PaymentMethod? paymentMethod = null
        )
        {
            if (!string.IsNullOrWhiteSpace(note))
            {
                Note = note;
            }

            if (Status != status && status.HasValue)
            {
                if (status == FundStatus.Confirmed)
                {
                    TransactionDate = DateTimeOffset.UtcNow;
                }
                Status = (FundStatus)status;
            }

            if (PaymentMethod != paymentMethod && paymentMethod.HasValue)
            {
                PaymentMethod = (PaymentMethod)paymentMethod;
            }
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
