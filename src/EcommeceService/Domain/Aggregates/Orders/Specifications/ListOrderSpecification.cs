using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Orders.Specifications
{
    public class ListOrderSpecification : Specification<Order>
    {
        public ListOrderSpecification(DateTime from, DateTime to)
        {
            Query
                .Where(x =>
                    x.OrderDate >= from && x.OrderDate < to
                )
                .Include(x => x.OrderItems)
                .Include(x => x.OrderPayments)
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
