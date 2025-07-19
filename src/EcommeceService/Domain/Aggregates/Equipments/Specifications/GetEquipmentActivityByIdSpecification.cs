using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications;

public class GetEquipmentActivityByIdSpecification : Specification<EquipmentActivity>
{
	public GetEquipmentActivityByIdSpecification(long id)
	{
		Query
			.Where(x => x.Id == id)
			.Include(x => x.Equipment)
			.Include(x => x.ActivityDetails)
			.Include(x => x.Staff)
			.AsSplitQuery();
	}
}
