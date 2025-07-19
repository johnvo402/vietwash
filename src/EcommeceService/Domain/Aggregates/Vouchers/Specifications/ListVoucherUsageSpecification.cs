using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications;

public class ListVoucherUsageSpecification : Specification<VoucherUsage>
{
    public ListVoucherUsageSpecification(long? customerId)
    {
        if (customerId.HasValue)
        {
            Query.Where(v => v.CustomerId == customerId.Value);
        }

        Query.AsNoTracking().AsSplitQuery();
    }
}
