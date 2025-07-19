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
				BranchId = activity.BranchId,
				StaffId = activity.StaffId,
				SupervisorName = activity.Staff.DisplayName,
				SupervisorCode = activity.Staff.Code,
				Type = activity.Type,
				LaborCost = activity.LaborCost,
				TotalCost = activity.TotalCost,
				Description = activity.Description,
			};
	}
}
