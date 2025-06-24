using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Equipments
{
    public class RepairDetail : DefaultEntity
    {
        public string PartName { get; set; } = default!;

        public int Quantity { get; set; } = default!;

        public decimal UnitPrice { get; set; } = default!;

        public decimal Amount { get; set; } = default!;

        public long RepairHistoryId { get; set; } = default!;

        public RepairHistory? RepairHistory { get; set; }
    }
}
