using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders.Enums
{
    public enum PaymentMethod : byte
    {
        Cash = 0,
        Card = 1
    }
}
