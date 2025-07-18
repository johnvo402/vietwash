using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Users.Enums;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Vouchers
{
    public class VoucherCustomerGroup : DefaultEntity<long>
    {
        public long VoucherId { get; set; }
        public Voucher Voucher { get; set; } = default!;
        public CustomerGroup Group { get; set; }
    }
}
