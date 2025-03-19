using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Services.Specifications;

public class ListServiceSpecification : Specification<Service>
{
    public ListServiceSpecification()
    {
        Query.AsNoTracking().AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
