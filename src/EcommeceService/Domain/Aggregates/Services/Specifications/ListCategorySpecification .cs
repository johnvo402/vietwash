using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Services.Specifications;

public class ListCategorySpecification : Specification<Category>
{
    public ListCategorySpecification()
    {
        Query.Where(x => !x.Disabled).AsNoTracking().AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
