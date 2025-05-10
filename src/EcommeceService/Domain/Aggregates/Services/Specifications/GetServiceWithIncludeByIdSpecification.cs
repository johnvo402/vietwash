using Domain.Aggregates.Services;
using JohnChum.SharedKernel.Domain.Common.Specs;

public class GetServiceWithIncludeByIdSpecification : Specification<Service>
{
    public GetServiceWithIncludeByIdSpecification(long id)
    {
        Query
            .Where(x => x.Id == id)
            .Include(x => x.UnitRelations)
            .ThenInclude(ur => ur.Unit)
            .Include(x => x.Category)
            .AsNoTracking();
    }
}
