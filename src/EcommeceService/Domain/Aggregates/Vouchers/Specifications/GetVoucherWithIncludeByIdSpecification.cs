using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetVoucherWithIncludeByIdSpecification : Specification<Voucher>
    {
        public GetVoucherWithIncludeByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id)
                .Include(x => x.VoucherCustomers)
                .Include(x => x.VoucherCustomerGroups);
        }
    }
}
