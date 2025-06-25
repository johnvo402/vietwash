using Specification;
using Specification.Builders;


namespace Domain.Aggregates.Products.Specifications
{
	public class GetProductWithIncludeByIdSpecification : Specification<Product>
	{
		public GetProductWithIncludeByIdSpecification(long id)
		{
			Query
				.Where(x => x.Id == id && !x.Disable)
				.AsNoTracking();
		}
	}
}
