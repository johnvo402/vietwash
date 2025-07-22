using Domain.Aggregates.Orders.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderByIdSpecification : Specification<Order>
    {
        public GetOrderByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id && x.Status == OrderStatus.Completed)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Service)
                .Include(x => x.Customer)
                .Include(x => x.Staff)
                .AsSplitQuery();
        }
    }
}
