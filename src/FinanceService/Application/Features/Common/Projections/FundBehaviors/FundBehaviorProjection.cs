using Contracts.Application.Common;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Enums;

namespace Application.Features.Common.Projections.FundBehaviors
{
    public class FundBehaviorProjection : BaseResponse<long>
    {
        public object Name { get; set; } = default!;

        public FundType Type { get; set; } = default!;

        public virtual void MappingFrom(FundBehavior fundBehavior)
        {
            Id = fundBehavior.Id;
            CreatedAt = fundBehavior.CreatedAt;
            CreatedBy = fundBehavior.CreatedBy;
            UpdatedAt = fundBehavior.UpdatedAt;
            UpdatedBy = fundBehavior.UpdatedBy;
            Name = fundBehavior.Name;
            Type = fundBehavior.Type;
        }
    }
}
