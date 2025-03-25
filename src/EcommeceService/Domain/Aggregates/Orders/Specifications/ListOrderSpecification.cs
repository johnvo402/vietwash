using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Orders.Specifications
{
    public class ListOrderSpecification : Specification<Order>
    {
        public ListOrderSpecification(string from, string to)
        {
            if (from != null || to != null)
            {
                Query
    .Where(x =>
        x.OrderDate >= DateTime.Parse(from) && x.OrderDate < DateTime.Parse(to)
    )
    .Include(x => x.OrderItems)
    .Include(x => x.OrderPayments)
    .AsNoTracking()
    .AsSplitQuery();
            }
            else
            {
                Query

        .Include(x => x.OrderItems)
        .Include(x => x.OrderPayments)
        .AsNoTracking()
        .AsSplitQuery();
            }
        }
    }
}
