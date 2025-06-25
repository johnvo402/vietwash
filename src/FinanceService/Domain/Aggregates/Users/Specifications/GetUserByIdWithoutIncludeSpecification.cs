using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdWithoutIncludeSpecification : Specification<User>
{
    public GetUserByIdWithoutIncludeSpecification(long id)
    {
        Query.Where(x => x.Id == id).AsNoTracking();
    }
}
