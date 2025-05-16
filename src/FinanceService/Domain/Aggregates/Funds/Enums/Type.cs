using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Funds.Enums
{
    public enum TransactionType : byte
    {
        point = 1,
        money = 2,
    }
}
