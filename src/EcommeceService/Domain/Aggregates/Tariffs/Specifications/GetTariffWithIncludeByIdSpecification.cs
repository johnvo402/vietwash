using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Tariffs.Specifications
{
	public class GetTariffWithIncludeByIdSpecification : Specification<Tariff>
	{
		public GetTariffWithIncludeByIdSpecification(long id)
		{
			Query
				.Where(x => x.Id == id && x.Disable == false)
				.Include(t => t.ServiceTariffs)
					.ThenInclude(st => st.Service)
				.Include(t => t.ServiceTariffs)
					.ThenInclude(st => st.UnitRelation);
		}
	}
}
