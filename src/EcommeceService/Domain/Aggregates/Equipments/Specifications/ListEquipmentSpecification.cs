using Specification;
using Specification.Builders;


namespace Domain.Aggregates.Equipments.Specifications
{
	public class ListEquipmentSpecification : Specification<Equipment>
	{
		public ListEquipmentSpecification()
		{
			Query
				.AsNoTracking()
				.AsSplitQuery();
			string key = GetUniqueCachedKey();
			Query.EnableCache(key);
		}
	}
}
