using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetVoucherByCodeSpecification : Specification<Voucher>
    {
        public GetVoucherByCodeSpecification(
            string code,
            long customerId,
            DateTimeOffset currentTime
        )
        {
            Query
                .Where(VoucherEligibility.ForCustomer(code, customerId, currentTime))
                .Include(x => x.VoucherCustomers);
        }
    }
}
