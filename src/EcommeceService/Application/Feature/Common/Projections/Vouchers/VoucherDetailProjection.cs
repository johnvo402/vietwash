using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Vouchers;

namespace Application.Feature.Common.Projections.Vouchers
{
    public class VoucherDetailProjection : VoucherProjection
    {
        public List<CustomerGroup> CustomerGroups { get; set; } = new();
        public List<long> CustomerIds { get; set; } = new();

        public override void MappingFrom(Voucher voucher)
        {
            base.MappingFrom(voucher);
            CustomerIds = voucher.VoucherCustomers.Select(x => x.CustomerId).Distinct().ToList();
        }
    }
}
