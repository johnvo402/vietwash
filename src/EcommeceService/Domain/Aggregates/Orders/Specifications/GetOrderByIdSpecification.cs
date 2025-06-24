using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderByIdSpecification : Specification<Order>
    {
        public GetOrderByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id)
                .Include(x => x.OrderItems)
                .Include(x => x.Customer)
                .Include(x => x.OrderPayments)
                .AsSplitQuery();
        }
    }
}
