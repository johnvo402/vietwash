using Ardalis.GuardClauses;
using Domain.Aggregates.Equipments.Enums;
using Mediator;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Equipments
{
    public class Equipment : AggregateRoot
    {
        public long BranchId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string? Image { get; set; }
        public string Code { get; set; } = default!;

        public decimal Price { get; set; } = default!;

        public decimal Capacity { get; set; } = default!;

        public DateTimeOffset? LastMaintenanceOrRepairDate { get; set; }

        public DateTimeOffset? NextMaintenanceDate { get; set; }

        public EquipmentStatus Status { get; set; } = default!;

        public ICollection<EquipmentActivity> EquipmentActivities { get; set; } = [];

        public Equipment(
            long branchId,
            string name,
            string code,
            decimal price,
            decimal capacity,
            EquipmentStatus status,
            string? description = null,
            DateTimeOffset? lastMaintenanceOrRepairDate = null,
            DateTimeOffset? nextMaintenanceDate = null
        )
        {
            BranchId = Guard.Against.NegativeOrZero(branchId, nameof(branchId));
            Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
            Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
            Price = price;
            Capacity = capacity;
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));
            Description = description?.Trim();
            LastMaintenanceOrRepairDate = lastMaintenanceOrRepairDate;
            NextMaintenanceDate = nextMaintenanceDate;
        }

        public void Update(
            long? branchId = null,
            string? name = null,
            string? description = null,
            string? code = null,
            decimal? price = null,
            decimal? capacity = null,
            EquipmentStatus? status = null,
            DateTimeOffset? lastMaintenanceOrRepairDate = null,
            DateTimeOffset? nextMaintenanceDate = null
        )
        {
            if (branchId.HasValue && branchId.Value > 0)
                BranchId = branchId.Value;
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();
            if (!string.IsNullOrWhiteSpace(code))
                Code = code.Trim();
            if (description != null)
                Description = description.Trim();
            if (price.HasValue)
                Price = price.Value;
            if (capacity.HasValue)
                Capacity = capacity.Value;
            if (status.HasValue)
                Status = status.Value;
            if (lastMaintenanceOrRepairDate.HasValue)
                LastMaintenanceOrRepairDate = lastMaintenanceOrRepairDate;
            if (nextMaintenanceDate.HasValue)
                NextMaintenanceDate = nextMaintenanceDate;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}
