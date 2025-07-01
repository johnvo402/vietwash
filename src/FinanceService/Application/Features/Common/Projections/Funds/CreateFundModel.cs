using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.Funds
{
    public class CreateFundModel
    {
        public FundType Type { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public long FundBehaviorId { get; set; }
        public string? Note { get; set; } = default!;
        public FundStatus Status { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
        public long BranchId { get; set; } = default!;
    }
}
