using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.Extensions;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;

namespace Application.Feature.EquipmentActivities.Command.Update
{
	public static class UpdateEquipmentActivityMapping
	{
		public static void FromUpdateModel(this EquipmentActivity entity, EquipmentActivityModel model, long staffId, string supervisorCode)
		{
			decimal amount = model.Details.Sum(d => d.UnitPrice * d.Quantity);
			entity.Update(
				branchId: model.BranchId,
				staffId: staffId,
				reportedDate: model.ReportedDate,
				scheduledDate: model.ScheduledDate,
				laborCost: model.LaborCost,
				totalCost: amount + model.LaborCost,
				description: model.Description,
				supervisorCode: supervisorCode
			);
			entity.ActivityDetails = model.Details.ToListMapping(x => new EquipmentActivityDetail
			{
				PartName = x.PartName,
				Quantity = x.Quantity,
				UnitPrice = x.UnitPrice,
				Amount = x.Quantity * x.UnitPrice
			});
		}
	}
}
