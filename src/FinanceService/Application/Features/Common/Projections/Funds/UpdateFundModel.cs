using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.Funds
{
    public class UpdateFundModel
    {
        public string? Note { get; set; }
        public FundStatus Status { get; set; } = default!;
        public PaymentMethod PaymentMethod { get; set; } = default!;
    }
}
