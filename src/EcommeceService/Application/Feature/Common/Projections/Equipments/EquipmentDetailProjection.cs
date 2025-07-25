using Domain.Aggregates.Equipments;

namespace Application.Feature.Common.Projections.Equipments
{
    public class EquipmentDetailProjection : EquipmentProjection
    {
        public DateTimeOffset? LastMaintenanceOrRepairDate { get; set; }
        public DateTimeOffset? NextMaintenanceDate { get; set; }

        public override void MappingFrom(Equipment equipment)
        {
            base.MappingFrom(equipment);
            LastMaintenanceOrRepairDate = equipment.LastMaintenanceOrRepairDate;
            NextMaintenanceDate = equipment.NextMaintenanceDate;
        }
    }
}
