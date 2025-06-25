using Domain.Aggregates.Orders.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderByCustomerIdSpecification : Specification<Order>
    {
        public GetOrderByCustomerIdSpecification(long customerId)
        {
            Query
                .Where(x => x.CustomerId == customerId && x.Status != OrderStatus.Cancelled)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Service)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.UnitRelation)
                .Include(x => x.OrderPayments)
                .AsSplitQuery();
        }
    }
}
