using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Branches.Specifications
{
    public class GetBranchByIdWithoutIncludeSpecification : Specification<Branch>
    {
        public GetBranchByIdWithoutIncludeSpecification(long id)
        {
            Query.Where(x => x.Id == id).AsNoTracking();
        }
    }
}
