using Shared.Kernel.Common;

namespace Domain.Aggregates.Equipments
{
    public class EquipmentActivityDetail : DefaultEntity
    {
        public string PartName { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public decimal UnitPrice { get; set; } = default!;
        public decimal Amount { get; set; } = default!;
        public long EquipmentActivityId { get; set; }
        public EquipmentActivity? EquipmentActivity { get; set; }
    }
}
