using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Equipments.Enums;
using JohnChum.SharedKernel.Domain.Common;
using Mediator;

namespace Domain.Aggregates.Equipments
{
    public class Equipment : AggregateRoot
    {
        public string Name { get; set; } = default!;

        public string Note { get; set; } = default!;

        public string Code { get; set; } = default!;

        public EquipmentType Type { get; set; } = default!;

        public decimal Price { get; set; } = default!;

        public decimal Discount { get; set; } = default!;

        public decimal Capacity { get; set; } = default!;

        public DateTimeOffset? LastMaintenanceDate { get; set; }

        public DateTimeOffset? NextMaintenanceDate { get; set; }

        public EquipmentStatus Status { get; set; } = default!;

        public long BranchId { get; set; } = default!;

        public string? Description { get; set; }
        public ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = [];
        public ICollection<RepairHistory> RepairHistories { get; set; } = [];

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
