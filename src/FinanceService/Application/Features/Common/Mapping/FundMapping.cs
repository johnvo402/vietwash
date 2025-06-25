using Application.Features.Common.Projections.FundBehaviors;
using Domain.Aggregates.Funds;

namespace Application.Features.Common.Mapping
{
    public static class FundMapping
    {
        public static FundBehaviorProjection ToFundBehaviorProjection(
            this FundBehavior fundBehavior
        )
        {
            var projection = new FundBehaviorProjection();
            projection.MappingFrom(fundBehavior);
            return projection;
        }
    }
}
