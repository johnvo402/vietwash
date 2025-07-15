using Application.Feature.Common.Projections.Equipments;
using Domain.Aggregates.Equipments;

namespace Application.Feature.Equipments.Command.Create
{
    public static class CreateEquipmentMapping
    {
        public static Equipment ToEntity(this EquipmentModel model)
        {
            return new Equipment(
                branchId: model.BranchId,
                name: model.Name,
                code: model.Code,
                price: model.Price,
                capacity: model.Capacity,
                status: model.Status,
                description: model.Description,
                lastMaintenanceDate: model.LastMaintenanceDate,
                nextMaintenanceDate: model.NextMaintenanceDate
            );
        }
    }
}
