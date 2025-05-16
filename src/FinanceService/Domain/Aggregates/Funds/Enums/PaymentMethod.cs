using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Funds.Enums
{
    public enum PaymentMethod : byte
    {
        cash = 1,
        banking = 2,
        card = 3,
    }
}
