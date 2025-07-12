using Application.Features.Common.Mapping;
using Application.Features.Common.Projections.Users;
using Contracts.Application.Common;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.Transactions
{
    public class TransactionProjection : BaseResponse
    {
        public long CustomerId { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public DateTimeOffset TransactionAt { get; set; } = default!;
        public TransactionType Type { get; set; } = default!;
        public object? Metadata { get; set; }
        public UserDTO? Customer { get; set; }
    }
}
