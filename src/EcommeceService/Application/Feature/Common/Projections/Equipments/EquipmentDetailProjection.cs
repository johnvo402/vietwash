using Domain.Aggregates.Equipments;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Common.Projections.Equipments
{
    public class EquipmentDetailProjection : EquipmentProjection
    {
        public DateTimeOffset? LastMaintenanceOrRepairDate { get; set; }
        public DateTimeOffset? NextMaintenanceDate { get; set; }
        public int NumberOfUses { get; set; }

        public override void MappingFrom(Equipment equipment)
        {
            base.MappingFrom(equipment);
            LastMaintenanceOrRepairDate = equipment.LastMaintenanceOrRepairDate;
            NextMaintenanceDate = equipment.NextMaintenanceDate;
            NumberOfUses = equipment
                .OrderEquipments.Where(x =>
                    x.Order.Status == OrderStatus.Processed
                    || x.Order.Status == OrderStatus.Completed
                )
                .Count();
        }
    }
}
