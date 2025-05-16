using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Equipments
{
    public class MaintenanceHistory : DefaultEntity
    {
        public DateTimeOffset MaintenanceDate { get; set; } = default!;

        public DateTimeOffset? NextMaintenanceDate { get; set; } = default!;

        public decimal Total { get; set; } = default!;

        public string? Description { get; set; } = default!;

        public string Supervisor { get; set; } = default!;

        public long EquipmentId { get; set; } = default!;

        public long BranchId { get; set; } = default!;

        public Equipment? Equipment { get; set; }

        public ICollection<MaintenanceDetail> MaintenanceDetails { get; set; } = [];
    }
}
