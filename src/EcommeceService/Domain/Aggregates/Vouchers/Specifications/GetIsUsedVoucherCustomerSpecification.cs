using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetIsUsedVoucherCustomerSpecification : Specification<VoucherCustomer>
    {
        public GetIsUsedVoucherCustomerSpecification(long customerId, long voucherId)
        {
            Query.Where(vc =>
                vc.VoucherId == voucherId && vc.CustomerId == customerId && vc.IsUsed == false
            );
        }
    }
}
