using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Equipments
{
    public class MaintenanceDetail : DefaultEntity
    {
        public string PartName { get; set; } = default!;

        public int Quantity { get; set; } = default!;

        public decimal UnitPrice { get; set; } = default!;

        public decimal Amount { get; set; } = default!;

        public long MaintenanceHistoryId { get; set; }

        public MaintenanceHistory? MaintenanceHistory { get; set; }
    }
}
