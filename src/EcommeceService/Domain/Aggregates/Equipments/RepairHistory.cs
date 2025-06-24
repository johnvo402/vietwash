using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Equipments
{
    public class RepairHistory : DefaultEntity
    {
        public DateTimeOffset RepairDate { get; set; } = default!;

        public string ReceivedBy { get; set; } = default!;

        public string? Description { get; set; } = default!;

        public decimal Total { get; set; } = default!;

        public long BranchId { get; set; } = default!;

        public long EquipmentId { get; set; } = default!;

        public Equipment? Equipment { get; set; } = default!;
        public ICollection<RepairDetail> RepairDetails { get; set; } = [];
    }
}
