using System.Linq.Expressions;
using Domain.Aggregates.Orders.Enums;
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
            List<string> branchs
        )
        {
            Expression<Func<Order, bool>> criteria = x => x.Status == OrderStatus.Completed;

            if (DateTime.TryParse(from, out var fromDate) && DateTime.TryParse(to, out var toDate))
            {
                criteria = x =>
                    x.Status == OrderStatus.Completed
                    && branchs.Contains(x.BranchId.ToString())
                    && x.OrderDate >= fromDate
                    && x.OrderDate < toDate;
            }
            else if (!string.IsNullOrEmpty(branchId) && long.TryParse(branchId, out var bid))
            {
                criteria = x =>
                    x.Status == OrderStatus.Completed
                    && branchs.Contains(x.BranchId.ToString())
                    && x.BranchId == bid;
            }

            Query
                .Where(criteria)
                .Include(x => x.OrderItems)
                .Include(x => x.OrderPayments)
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
