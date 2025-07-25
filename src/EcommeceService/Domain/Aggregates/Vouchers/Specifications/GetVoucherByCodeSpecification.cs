using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetVoucherByCodeSpecification : Specification<Voucher>
    {
        public GetVoucherByCodeSpecification(string code, long customerId)
        {
            Query
                .Where(x =>
                    x.Code == code
                    && x.VoucherCustomers.Any(x => x.CustomerId == customerId && !x.IsUsed)
                )
                .Include(x => x.VoucherCustomers);
            ;
        }
    }
}
