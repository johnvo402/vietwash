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
				image: model.Image,
				code: model.Code,
				type: model.Type,
				price: model.Price,
				capacity: model.Capacity,
				status: model.Status,
				lastMaintenanceDate: model.LastMaintenanceDate,
				nextMaintenanceDate: model.NextMaintenanceDate
			);
		}
	}
}
