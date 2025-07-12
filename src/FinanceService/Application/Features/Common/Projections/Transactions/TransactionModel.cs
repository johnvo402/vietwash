using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.Transactions
{
    public class TransactionModel
    {
        public long CustomerId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public DateTimeOffset TransactionAt { get; set; } = default!;
        public TransactionType Type { get; set; } = default!;
        public object? Metadata { get; set; }
    }
}
