using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Products.Specifications
{
	public class ListProductSpecification : Specification<Product>
	{
		public ListProductSpecification()
		{
			Query.Where(x => !x.Disable).AsNoTracking().AsSplitQuery();
			string key = GetUniqueCachedKey();
			Query.EnableCache(key);
		}
	}
}
