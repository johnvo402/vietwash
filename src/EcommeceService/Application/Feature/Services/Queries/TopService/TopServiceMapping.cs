using System.Linq.Expressions;
using Domain.Aggregates.Orders;

namespace Application.Feature.Services.Queries.TopService
{
    public static class TopServiceMapping
    {
        public static Expression<Func<IGrouping<long, OrderItem>, TopServiceResponse>> Selector()
        {
            return group => new TopServiceResponse
            {
                Id = group.Key,
                Name = group.Select(x => x.Service.Name).FirstOrDefault() ?? string.Empty,
                Description =
                    group.Select(x => x.Service.Description).FirstOrDefault() ?? string.Empty,
                Image = group.Select(x => x.Service.Image).FirstOrDefault(),
                TotalUsed = group.Sum(x => x.Quantity),
                BasePrice = group
                    .SelectMany(x => x.Service.UnitRelations)
                    .Where(x => x.BaseUnit)
                    .Select(x => x.Price)
                    .FirstOrDefault(),
            };
        }
    }
}
