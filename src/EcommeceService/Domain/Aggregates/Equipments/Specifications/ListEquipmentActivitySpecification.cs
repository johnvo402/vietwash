using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications;

public class ListEquipmentActivitySpecification : Specification<EquipmentActivity>
{
	public ListEquipmentActivitySpecification()
	{
		Query
			.Include(x => x.Equipment)
			.Include(x => x.ActivityDetails)
			.AsNoTracking()
			.AsSplitQuery();
		string key = GetUniqueCachedKey();
		Query.EnableCache(key);
	}
}
