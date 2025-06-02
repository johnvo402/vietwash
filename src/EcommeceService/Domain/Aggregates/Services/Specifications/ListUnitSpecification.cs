using JohnChum.SharedKernel.Domain.Common.Specs;


namespace Domain.Aggregates.Services.Specifications
{
	public class ListUnitSpecification : Specification<Unit>
	{
		public ListUnitSpecification()
		{
			Query
				.AsNoTracking()
				.AsSplitQuery();
			string key = GetUniqueCachedKey();
			Query.EnableCache(key);
		}
	}
}
