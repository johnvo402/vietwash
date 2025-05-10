using JohnChum.SharedKernel.Domain.Common.Specs;


namespace Domain.Aggregates.Services.Specifications
{
	public class GetUnitByIdWithoutIncludeSpecification : Specification<Unit>
	{
		public GetUnitByIdWithoutIncludeSpecification(Ulid id)
		{
			Query.Where(x => x.Id == id).AsNoTracking();
		}
	}
}
