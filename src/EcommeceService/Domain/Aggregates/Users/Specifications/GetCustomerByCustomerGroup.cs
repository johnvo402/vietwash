using Domain.Aggregates.Users.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Users.Specifications
{
    public class GetCustomerByCustomerGroups : Specification<User>
    {
        public GetCustomerByCustomerGroups(IEnumerable<CustomerGroup> groups)
        {
            Query
                .Where(user =>
                    user.CustomerGroup.HasValue && groups.Contains(user.CustomerGroup.Value)
                )
                .AsNoTracking();
        }
    }
}
