using Domain.Aggregates.Funds.Enums;
using JohnChum.SharedKernel.Application.Common;

namespace Application.Features.Common.Projections.FundBehaviors
{
    public class FundBehaviorProjection : BaseResponse<long>
    {
        public string Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;
    }
}
