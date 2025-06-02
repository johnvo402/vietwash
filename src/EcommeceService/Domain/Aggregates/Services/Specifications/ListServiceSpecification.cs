using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Services.Specifications;

public class ListServiceSpecification : Specification<Service>
{
    public ListServiceSpecification()
    {
        Query.Where(x=>!x.Disable).Include(x=>x.Category).Include(x=>x.UnitRelations).AsNoTracking().AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
