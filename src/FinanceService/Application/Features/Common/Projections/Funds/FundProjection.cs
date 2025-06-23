using Application.Features.Common.Projections.Users;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;
using JohnChum.SharedKernel.Application.Common;

namespace Application.Features.Common.Projections.Funds
{
    public class FundProjection : BaseResponse
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public FundType Type { get; set; }
        public FundStatus Status { get; set; }
        public decimal Amount { get; set; }
        public long FundBehaviorId { get; set; }
        public DateTimeOffset? TransactionDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public long BranchId { get; set; }
        public FundBehavior? FundBehavior { get; set; }
        public long ObjectId { get; set; }
        public object? Metadata { get; set; }
        public long? ReferenceId { get; set; }
        public UserDTO? User { get; set; }

    }
}
