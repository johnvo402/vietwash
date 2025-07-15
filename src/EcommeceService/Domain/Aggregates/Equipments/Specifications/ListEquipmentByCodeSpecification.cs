using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications
{
    public class ListEquipmentByCodeSpecification : Specification<Equipment>
    {
        public ListEquipmentByCodeSpecification(List<string> codes)
        {
            Query.Where(x => codes.Contains(x.Code)).AsNoTracking().AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
