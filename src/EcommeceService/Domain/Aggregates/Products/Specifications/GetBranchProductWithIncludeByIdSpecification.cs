using Specification;
using Specification.Builders;


namespace Domain.Aggregates.Products.Specifications
{
	public class GetBranchProductWithIncludeByIdSpecification : Specification<BranchProduct>
	{
		public GetBranchProductWithIncludeByIdSpecification(long id)
		{
			Query
				.Where(x => x.Id == id && !x.Disable)
				.Include(x => x.UnitRelations);
		}
	}
}
