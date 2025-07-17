using Domain.Aggregates.Users.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class ListUserByRoleIncludeSpecification : Specification<User>
{
    public ListUserByRoleIncludeSpecification(List<string> roles)
    {
        Query.Where(x => roles.Contains(x.Role)).AsNoTracking();
    }
}
