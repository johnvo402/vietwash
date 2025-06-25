using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Branches.Specifications
{
    public class ListBranchSpecification : Specification<Domain.Aggregates.Branches.Branch>
    {
        public ListBranchSpecification()
        {
            Query.Where(x=> !x.Disable).AsNoTracking().AsSplitQuery();
            string key = GetUniqueCachedKey();
            Query.EnableCache(key);
        }
    }
}
