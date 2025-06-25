using System.Linq.Expressions;
using Domain.Aggregates.Funds;

namespace Application.Features.FundBehaviors.Queries
{
    public static class ListFundBehaviorMapping
    {
        public static Expression<Func<FundBehavior, ListFundBehaviorResponse>> Selector()
        {
            return service => new ListFundBehaviorResponse
            {
                Id = service.Id,
                CreatedAt = service.CreatedAt,
                CreatedBy = service.CreatedBy,
                UpdatedAt = service.UpdatedAt,
                UpdatedBy = service.UpdatedBy,

                // Từ ServiceProjection
                Name = service.Name,
                Type = service.Type,
            };
        }
    }
}
