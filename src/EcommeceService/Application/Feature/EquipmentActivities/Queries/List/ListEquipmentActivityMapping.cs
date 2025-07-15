using Domain.Aggregates.Equipments;
using System.Linq.Expressions;

namespace Application.Feature.EquipmentActivities.Queries.List
{
	public static class ListEquipmentActivityMapping
	{
		public static Expression<Func<EquipmentActivity, ListEquipmentActivityResponse>> Selector() =>
			activity => new ListEquipmentActivityResponse
			{
				Id = activity.Id,
				PublicId = activity.PublicId,
				CreatedAt = activity.CreatedAt,
				CreatedBy = activity.CreatedBy,
				UpdatedAt = activity.UpdatedAt,
				UpdatedBy = activity.UpdatedBy,

				EquipmentId = activity.EquipmentId,
				EquipmentName = activity.Equipment != null ? activity.Equipment.Name : null,
				Image = activity.Equipment != null ? activity.Equipment.Image : null,
				BranchId = activity.BranchId,
				StaffId = activity.StaffId,
				SupervisorCode = activity.SupervisorCode,
				Type = activity.Type,
				ReportedDate = activity.ReportedDate,
				ScheduledDate = activity.ScheduledDate,
				LaborCost = activity.LaborCost,
				TotalCost = activity.TotalCost,
				Description = activity.Description,
				Status = activity.Status
			};
	}
}
