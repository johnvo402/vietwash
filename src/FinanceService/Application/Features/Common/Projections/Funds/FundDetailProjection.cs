using Application.Features.Common.Projections.Users;

namespace Application.Features.Common.Projections.Funds
{
    public class FundDetailProjection : FundProjection
    {
        public string? Note { get; set; }
    }
}
