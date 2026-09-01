using System.Linq.Expressions;
using Shared.Kernel.Extentions;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications;

public class ListOrderSpecification : Specification<Order>
{
    public ListOrderSpecification(
        string? from,
        string? to,
        long? branchId,
        IReadOnlyCollection<long> branchIds,
        long? customerId = null,
        long? serviceId = null
    )
    {
        Expression<Func<Order, bool>> criteria = x => branchIds.Contains(x.BranchId);

        if (DateTime.TryParse(from, out DateTime fromDate) && DateTime.TryParse(to, out DateTime toDate))
            criteria = criteria.And(x => x.OrderDate >= fromDate && x.OrderDate < toDate);

        if (branchId.HasValue)
            criteria = criteria.And(x => x.BranchId == branchId.Value);

        if (customerId is not null)
            criteria = criteria.And(x => x.CustomerId == customerId);

        if (serviceId is not null)
            criteria = criteria.And(x => x.OrderItems.Any(oi => oi.ServiceId == serviceId));

        Query
            .Where(criteria)
            .Include(x => x.OrderItems)
            .Include(x => x.Customer)
            .AsNoTracking()
            .AsSplitQuery();
    }
}
