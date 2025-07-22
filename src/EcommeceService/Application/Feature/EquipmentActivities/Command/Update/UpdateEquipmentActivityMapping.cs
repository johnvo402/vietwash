using Application.Feature.Common.Mapping.EquipmentActivities;
using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.Extensions;
using Domain.Aggregates.Equipments;

namespace Application.Feature.EquipmentActivities.Command.Update
{
	public static class UpdateEquipmentActivityMapping
	{
		public static void FromUpdateModel(this EquipmentActivity entity, EquipmentActivityModel model, long staffId)
		{
			decimal amount = model.Details.Sum(d => d.UnitPrice * d.Quantity);
			entity.Update(
				staffId: staffId,
				type: model.Type,
				laborCost: model.LaborCost,
				totalCost: amount + model.LaborCost,
				description: model.Description
			);
			entity.ActivityDetails = model.Details.ToListMapping(x => new EquipmentActivityDetail
			{
				PartName = x.PartName,
				Quantity = x.Quantity,
				UnitPrice = x.UnitPrice,
				Amount = x.Quantity * x.UnitPrice
			});
			entity.ActivityDetails = model.Details.ToListActivityDetails() ?? [];
		}
	}
}
