using System.Linq.Expressions;
using Domain.Aggregates.Equipments;

namespace Application.Feature.Equipments.Queries.List
{
    public class ListEquipmentMapping
    {
        public static Expression<Func<Equipment, ListEquipmentResponse>> Selector()
        {
            return equipment => new ListEquipmentResponse
            {
                Id = equipment.Id,
                PublicId = equipment.PublicId,
                CreatedAt = equipment.CreatedAt,
                CreatedBy = equipment.CreatedBy,
                UpdatedAt = equipment.UpdatedAt,
                UpdatedBy = equipment.UpdatedBy,

                BranchId = equipment.BranchId,
                Name = equipment.Name,
                Image = equipment.Image,
                Description = equipment.Description,
                Code = equipment.Code,
                Price = equipment.Price,
                Capacity = equipment.Capacity,
                Status = equipment.Status,
            };
        }
    }
}
