using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Users.Specifications;

public class ListUserSpecification : Specification<User>
{
    public ListUserSpecification()
    {
        Query
            .AsNoTracking()
            .AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
