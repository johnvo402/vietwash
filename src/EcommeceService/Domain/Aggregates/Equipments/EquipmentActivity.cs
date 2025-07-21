using Ardalis.GuardClauses;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Users;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Equipments
{
    public class EquipmentActivity : BaseEntity
    {
        public long EquipmentId { get; set; } = default!;
        public long StaffId { get; set; } = default!;
        public TypeActivity Type { get; set; } = default!;
        public decimal LaborCost { get; set; } // tiền công
        public decimal TotalCost { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public Equipment? Equipment { get; set; }
        public User? Staff { get; set; }
        public ICollection<EquipmentActivityDetail> ActivityDetails { get; set; } = [];

        public EquipmentActivity(
            long equipmentId,
            long staffId,
            TypeActivity type,
            decimal laborCost,
            decimal totalCost,
            string? description
        )
        {
            Guard.Against.NegativeOrZero(equipmentId, nameof(equipmentId));
            Guard.Against.NegativeOrZero(staffId, nameof(staffId));

            EquipmentId = equipmentId;
            StaffId = staffId;
            Type = type;
            LaborCost = laborCost;
            TotalCost = totalCost;
            Description = description;
        }
    }
}
