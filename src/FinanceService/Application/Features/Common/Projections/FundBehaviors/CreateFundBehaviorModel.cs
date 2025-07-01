using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.FundBehaviors
{
    public class CreateFundBehaviorModel
    {
        public object Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;
    }
}
