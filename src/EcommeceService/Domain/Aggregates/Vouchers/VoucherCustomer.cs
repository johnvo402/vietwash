using Domain.Aggregates.Users;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Vouchers
{
    public class VoucherCustomer : DefaultEntity<long>
    {
        public long CustomerId { get; set; } = default!;
        public long VoucherId { get; set; } = default!;
        public bool IsUsed { get; set; } = default!;

        public Voucher? Voucher { get; set; }
        public User? Customer { get; set; }
    }
}
