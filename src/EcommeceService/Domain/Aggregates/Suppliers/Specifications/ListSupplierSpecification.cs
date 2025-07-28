using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Suppliers.Specifications
{
    public class ListSupplierSpecification : Specification<Supplier>
    {
        public ListSupplierSpecification()
        {
            Query
                .Where(x => !x.Disable)
                .Include(x => x.ProductSupplyings)
                .AsNoTracking()
                .AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
