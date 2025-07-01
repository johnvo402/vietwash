using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders.Enums
{
    public enum OrderStatus : byte
    {
        Pending = 0,
        InProgress = 1,
        Processed = 2,
        Completed = 3,
        Cancelled = 4,
    }
}
