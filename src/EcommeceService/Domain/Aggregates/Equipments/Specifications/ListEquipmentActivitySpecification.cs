using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications;

public class ListEquipmentActivitySpecification : Specification<EquipmentActivity>
{
	public ListEquipmentActivitySpecification()
	{
		Query
			.Include(x => x.Staff)
			.AsNoTracking()
			.AsSplitQuery();
		string key = GetUniqueCachedKey();
		Query.EnableCache(key);
	}
}
