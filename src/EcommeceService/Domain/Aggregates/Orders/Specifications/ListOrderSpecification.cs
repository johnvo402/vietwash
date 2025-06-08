using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Orders.Specifications
{
    public class ListOrderSpecification : Specification<Order>
    {
        public ListOrderSpecification(string? from, string? to, string? branchId)
        {
            Query
                .Where(x =>
                        x.Status.Equals("Completed")

                                            )
                    .Include(x => x.OrderItems)
                    .Include(x => x.OrderPayments)
            .AsNoTracking()
                    .AsSplitQuery();

            if (!string.IsNullOrEmpty(from) || !string.IsNullOrEmpty(to))
            {
                Query
                    .Where(x =>
                        x.OrderDate >= DateTime.Parse(from)
                        && x.OrderDate < DateTime.Parse(to)
                                            );

            }
            else if (!string.IsNullOrEmpty(branchId))
            {
                Query
                    .Where(x => x.BranchId == Int32.Parse(branchId));

            }
        }
    }
}
