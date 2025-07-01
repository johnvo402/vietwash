using Domain.Aggregates.Services;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Services.Specifications;

public class GetServiceWithIncludeByIdSpecification : Specification<Service>
{
    public GetServiceWithIncludeByIdSpecification(long id)
    {
        Query
            .Where(x => x.Id == id && x.Disable == false)
            .Include(x => x.UnitRelations)
            .Include(x => x.Category);
    }
}
