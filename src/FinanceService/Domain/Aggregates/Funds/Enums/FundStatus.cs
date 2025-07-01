using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Funds.Enums
{
    public enum FundStatus : byte
    {
        PendingConfirmation = 1,
        Confirmed = 2,
        Cancelled = 3
    }
}
