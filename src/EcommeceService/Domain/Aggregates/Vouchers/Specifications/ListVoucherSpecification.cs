using Domain.Aggregates.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class ListVoucherSpecification : Specification<Voucher>
    {
        public ListVoucherSpecification(long? customerId)
        {
            if (customerId.HasValue)
            {
                Query.Where(v =>
                    v.VoucherCustomers.Any(vc =>
                        vc.CustomerId == customerId.Value && vc.IsUsed == false
                    )
                    && v.Status == ActivationStatus.Active
                );
            }

            Query.Include(v => v.VoucherCustomers).AsNoTracking().AsSplitQuery();
        }
    }
}
