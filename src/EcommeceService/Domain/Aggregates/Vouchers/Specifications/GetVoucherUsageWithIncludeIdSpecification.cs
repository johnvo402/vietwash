using Specification;
using Specification.Builders;


namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetVoucherUsageDetailByIdSpecification : Specification<VoucherUsage>
    {
        public GetVoucherUsageDetailByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id)
                .Include(x => x.Order)
                               ;
        }
    }
}
