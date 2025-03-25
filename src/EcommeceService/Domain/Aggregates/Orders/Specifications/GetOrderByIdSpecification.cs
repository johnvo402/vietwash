using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderByIdSpecification : Specification<Order>
    {
        public GetOrderByIdSpecification(Ulid id)
        {
            Query
                .Where(x => x.Id == id && x.Status != OrderStatus.Cancelled)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.Service)
                .Include(x => x.OrderItems)
                .ThenInclude(x => x.UnitRelation)
                .Include(x => x.OrderPayments)
                .AsSplitQuery();
        }
    }
}
