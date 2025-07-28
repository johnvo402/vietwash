using System.Linq.Expressions;
using Shared.Kernel.Extentions;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class ListOrderSpecification : Specification<Order>
    {
        public ListOrderSpecification(
            string? from,
            string? to,
            string? branchId,
            List<string> branchs,
            long? customerId = null,
            long? serviceId = null
        )
        {
            Expression<Func<Order, bool>> criteria = x => branchs.Contains(x.BranchId.ToString());

            if (DateTime.TryParse(from, out var fromDate) && DateTime.TryParse(to, out var toDate))
            {
                criteria = criteria.And(x => x.OrderDate >= fromDate && x.OrderDate < toDate);
            }

            if (!string.IsNullOrEmpty(branchId) && long.TryParse(branchId, out var bid))
            {
                criteria = criteria.And(x => x.BranchId == bid);
            }
            if (customerId != null)
            {
                criteria = criteria.And(x => x.CustomerId == customerId);
            }

            if (serviceId != null)
            {
                criteria = criteria.And(x => x.OrderItems.Any(oi => oi.ServiceId == serviceId));
            }

            Query
                .Where(criteria)
                .Include(x => x.OrderItems)
                .Include(x => x.Customer)
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
