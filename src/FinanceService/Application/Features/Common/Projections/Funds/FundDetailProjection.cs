using Domain.Aggregates.Funds;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Projections.Funds
{
    public class FundDetailProjection : FundProjection
    {
        public override void MappingFrom(Fund fund)
        {
            base.MappingFrom(fund);
        }
    }
}
