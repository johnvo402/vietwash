using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class ListRefreshtokenByFamillyIdSpecification : Specification<AccountToken>
{
    public ListRefreshtokenByFamillyIdSpecification(string familyId, long userId)
    {
        Query.Where(x => x.FamilyId == familyId && x.AccountId == userId);
    }
}
