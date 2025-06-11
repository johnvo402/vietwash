using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System.Linq.Expressions;

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
            Expression<Func<Order, bool>> criteria = x =>
    x.Status == OrderStatus.Completed ;

            if (DateTime.TryParse(from, out var fromDate) && DateTime.TryParse(to, out var toDate))
            {
                criteria = x =>
                    x.Status == OrderStatus.Completed &&
                    branchs.Contains(x.BranchId.ToString()) &&
                    x.OrderDate >= fromDate && x.OrderDate < toDate;
            }
            else if (!string.IsNullOrEmpty(branchId) && long.TryParse(branchId, out var bid))
            {
                criteria = x =>
                    x.Status == OrderStatus.Completed &&
                    branchs.Contains(x.BranchId.ToString()) &&
                    x.BranchId == bid;
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
