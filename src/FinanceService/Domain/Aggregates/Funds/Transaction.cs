using Ardalis.GuardClauses;
using Domain.Aggregates.Funds.Enums;
using Domain.Aggregates.Users;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Funds
{
    public class Transaction : BaseEntity
    {
        public long CustomerId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public DateTimeOffset TransactionAt { get; set; } = default!;
        public TransactionType Type { get; set; } = default!;
        public object? Metadata { get; set; }
        public User? Customer { get; set; }

        public Transaction(
            long customerId,
            decimal amount,
            DateTimeOffset transactionAt,
            TransactionType type,
            object? metadata
        )
        {
            CustomerId = Guard.Against.NegativeOrZero(customerId, nameof(customerId));
            Amount = Guard.Against.NegativeOrZero(amount, nameof(amount));
            TransactionAt = Guard.Against.Default(transactionAt, nameof(transactionAt));
            Type = Guard.Against.EnumOutOfRange(type, nameof(type));
            Metadata = metadata;
        }
    }
}
