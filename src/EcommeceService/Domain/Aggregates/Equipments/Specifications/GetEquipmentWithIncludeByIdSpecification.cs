using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications
{
    public class GetEquipmentWithIncludeByIdSpecification : Specification<Equipment>
    {
        public GetEquipmentWithIncludeByIdSpecification(long id)
        {
            Query
                .Where(x => x.Id == id)
                .Include(x => x.EquipmentActivities)
                .ThenInclude(a => a.ActivityDetails)
                .Include(x => x.OrderEquipments);
        }
    }
}
