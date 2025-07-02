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
                Name = group.First().Service.Name,
                Description = group.First().Service.Description,
                Image = group.First().Service.Image,
                TotalUsed = group.Sum(x => x.Quantity),
            };
        }
    }
}
