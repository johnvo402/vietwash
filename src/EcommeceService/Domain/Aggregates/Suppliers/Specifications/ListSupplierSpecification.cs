using JohnChum.SharedKernel.Domain.Common.Specs;


namespace Domain.Aggregates.Suppliers.Specifications
{
    public class ListSupplierSpecification : Specification<Supplier>
    {
        public ListSupplierSpecification()
        {
            Query.Where(x => !x.Disable).AsNoTracking().AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
