using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications
{
    public class GetEquipmentWithIncludeByIdSpecification : Specification<Equipment>
    {
        public GetEquipmentWithIncludeByIdSpecification(long id)
        {
            Query.Where(x => x.Id == id);
        }
    }
}
