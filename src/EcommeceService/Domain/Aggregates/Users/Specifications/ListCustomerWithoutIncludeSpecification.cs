using Domain.Aggregates.Users.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications;

public class ListCustomerWithoutIncludeSpecification : Specification<User>
{
    public ListCustomerWithoutIncludeSpecification(CustomerGroup group)
    {
        Query.Where(x => x.CustomerGroup == group).AsNoTracking();
    }
}
