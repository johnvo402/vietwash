using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Services.Specifications;

public class ListServiceSpecification : Specification<Service>
{
    public ListServiceSpecification()
    {
        Query
            .Where(x => !x.Disable)
            .Include(x => x.Category)
            .Include(x => x.UnitRelations)
            .ThenInclude(x => x.AsUnitProduct)
            .ThenInclude(x => x.BranchProduct)
            .AsNoTracking()
            .AsSplitQuery();
        string key = GetUniqueCachedKey();
        Query.EnableCache(key);
    }
}
