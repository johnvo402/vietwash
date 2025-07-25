using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Vouchers.Specifications
{
    public class GetExpiredVouchers : Specification<Voucher>
    {
        public GetExpiredVouchers(DateTimeOffset now)
        {
            Query.Where(x => (x.Status.Equals(0)) && x.EndAt < now);
        }
    }
}
