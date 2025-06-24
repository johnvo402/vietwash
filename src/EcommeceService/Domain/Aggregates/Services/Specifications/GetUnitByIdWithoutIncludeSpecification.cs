using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Services.Specifications
{
    public class GetUnitByIdWithoutIncludeSpecification : Specification<Unit>
    {
        public GetUnitByIdWithoutIncludeSpecification(long id)
        {
            Query.Where(x => x.Id == id).AsNoTracking();
        }
    }
}
