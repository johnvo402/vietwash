using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetVoucherByCodeSpecification : Specification<Voucher>
    {
        public GetVoucherByCodeSpecification(string code)
        {
            Query
                .Where(x => x.Code == code);
            ;
        }
    }
}
