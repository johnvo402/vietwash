using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.Funds
{
    public class UpdateFundModel
    {
        public string? Note { get; set; }
        public FundStatus? Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}
