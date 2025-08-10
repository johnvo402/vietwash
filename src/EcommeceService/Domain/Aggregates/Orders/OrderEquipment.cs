using Domain.Aggregates.Equipments;
using Shared.Kernel.Common;

namespace Domain.Aggregates.Orders
{
    public class OrderEquipment : DefaultEntity<long>
    {
        public long OrderId { get; set; }
        public long EquipmentId { get; set; }
        public string EquipmentName { get; set; } = default!;
        public Order Order { get; set; } = default!;
        public Equipment Equipment { get; set; } = default!;
    }
}
