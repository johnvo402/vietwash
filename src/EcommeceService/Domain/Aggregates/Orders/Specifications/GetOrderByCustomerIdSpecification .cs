using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderByCustomerIdSpecification : Specification<Order>
    {
        public GetOrderByCustomerIdSpecification(Ulid customerId)
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
