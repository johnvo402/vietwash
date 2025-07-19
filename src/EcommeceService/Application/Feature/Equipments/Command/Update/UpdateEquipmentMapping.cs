using Application.Feature.Common.Projections.Equipments;
using Domain.Aggregates.Equipments;

namespace Application.Feature.Equipments.Command.Update
{
    public static class UpdateEquipmentMapping
    {
        public static void FromUpdateModel(this Equipment entity, EquipmentModel model)
        {
            entity.Update(
                branchId: model.BranchId,
                name: model.Name,
                description: model.Description,
                code: model.Code,
                price: model.Price,
                capacity: model.Capacity,
                status: model.Status,
				lastMaintenanceOrRepairDate: model.LastMaintenanceOrRepairDate,
                nextMaintenanceDate: model.NextMaintenanceDate
            );
        }
    }
}
