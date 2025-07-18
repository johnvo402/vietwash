using Domain.Aggregates.Users;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Vouchers
{
    public class VoucherCustomer : DefaultEntity<long>
    {
        public long VoucherId { get; set; }
        public long CustomerId { get; set; }

        public Voucher Voucher { get; set; } = default!;
        public User Customer { get; set; } = default!;
    }
}
