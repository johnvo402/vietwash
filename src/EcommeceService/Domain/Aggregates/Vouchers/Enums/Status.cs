using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Vouchers.Enums
{
    public enum VoucherStatus : byte
    {
        active = 0,
        inactive = 1
    }
}
