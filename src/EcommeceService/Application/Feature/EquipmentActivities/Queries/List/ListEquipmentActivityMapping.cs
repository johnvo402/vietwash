using System.Linq.Expressions;
using Domain.Aggregates.Equipments;

namespace Application.Feature.EquipmentActivities.Queries.List
{
    public static class ListEquipmentActivityMapping
    {
        public static Expression<
            Func<EquipmentActivity, ListEquipmentActivityResponse>
        > Selector() =>
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
                StaffId = activity.StaffId,
                SupervisorName = activity.Staff != null ? activity.Staff.DisplayName : null,
                SupervisorCode = activity.Staff != null ? activity.Staff.Code : null,
                Type = activity.Type,
                LaborCost = activity.LaborCost,
                TotalCost = activity.TotalCost,
                Description = activity.Description,
            };
    }
}
