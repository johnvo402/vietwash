using Microsoft.EntityFrameworkCore;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class ListAccountSpecification : Specification<Account>
{
    public ListAccountSpecification(string[] roles)
    {
        Query.Where(x => roles.Contains(x.Role) && !x.Disabled).AsNoTracking().AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
