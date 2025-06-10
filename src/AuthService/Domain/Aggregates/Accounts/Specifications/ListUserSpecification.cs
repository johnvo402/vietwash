using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Accounts.Specifications;

public class ListAccountSpecification : Specification<Account>
{
    public ListAccountSpecification()
    {
        Query.Where(x => !x.Disabled).AsNoTracking().AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
