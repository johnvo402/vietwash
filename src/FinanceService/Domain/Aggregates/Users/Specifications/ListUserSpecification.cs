using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class ListUserSpecification : Specification<User>
{
    public ListUserSpecification(string[] roles)
    {
        Query
            .Where(x => roles.Contains(x.Role) && !x.Disabled)
            .Include(x => x.BranchUsers)
            .AsNoTracking()
            .AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
