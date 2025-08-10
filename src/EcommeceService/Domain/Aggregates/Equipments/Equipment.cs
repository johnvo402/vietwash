using Ardalis.GuardClauses;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Events;
using Domain.Events.Enums;
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

        public DateTimeOffset? LastMaintenanceOrRepairDate { get; set; }

        public DateTimeOffset? NextMaintenanceDate { get; set; }

        public EquipmentStatus Status { get; set; } = default!;

        public ICollection<EquipmentActivity> EquipmentActivities { get; set; } = [];

        public ICollection<OrderEquipment> OrderEquipments { get; set; } = [];

        public Equipment(
            long branchId,
            string name,
            string code,
            decimal price,
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
            Status = Guard.Against.EnumOutOfRange(status, nameof(status));
            Description = description?.Trim();
            LastMaintenanceOrRepairDate = lastMaintenanceOrRepairDate;
            NextMaintenanceDate = nextMaintenanceDate;
        }

        public void AddActivity(EquipmentActivity equipmentActivities)
        {
            this.EquipmentActivities.Add(equipmentActivities);
            Emit(
                new CreateFundEvent()
                {
                    TypeId = "spend",
                    ReferenceId = Id,
                    Amount = equipmentActivities.TotalCost,
                    PaymentMethod = PaymentMethod.Cash,
                    BranchId = BranchId,
                    BehaviorId = 7,
                    Metadata = new Dictionary<string, object>
                    {
                        ["code"] = Code,
                        ["publicId"] = PublicId.ToString(),
                        ["type"] = FundEventType.EquipmentActivity,
                    },
                    Point = 0,
                    FundEventType = FundEventType.EquipmentActivity,
                }
            );
        }

        public void Update(
            string? name = null,
            string? description = null,
            EquipmentStatus? status = null
        )
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();

            if (description != null)
                Description = description.Trim();

            if (status.HasValue)
                Status = status.Value;
        }

        protected override bool TryApplyDomainEvent(INotification domainEvent)
        {
            switch (domainEvent)
            {
                case CreateFundEvent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
