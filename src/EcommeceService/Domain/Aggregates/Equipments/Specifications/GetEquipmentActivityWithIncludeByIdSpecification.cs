using Domain.Aggregates.Equipments.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Equipments.Specifications
{
	public class GetEquipmentActivityWithIncludeByIdSpecification : Specification<EquipmentActivity>
	{
		public GetEquipmentActivityWithIncludeByIdSpecification(long id)
		{
			Query
				.Where(x => x.Id == id && x.Status != ActivityStatus.Done && x.Status != ActivityStatus.Cancelled)
				.Include(x => x.ActivityDetails);
		}
	}
}
