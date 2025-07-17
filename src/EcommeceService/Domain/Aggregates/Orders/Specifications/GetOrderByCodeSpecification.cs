using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderByCodeSpecification : Specification<Order>
    {
        public GetOrderByCodeSpecification(string code)
        {
            Query
                .Where(x => x.Code == code)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Service)
                .Include(x => x.Customer)
                .Include(x => x.Staff)
                .AsSplitQuery();
        }
    }
}
