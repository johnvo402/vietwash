using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Users.Specifications;

public class ListRefreshtokenByFamillyIdSpecification : Specification<UserToken>
{
    public ListRefreshtokenByFamillyIdSpecification(string familyId, long userId)
    {
        Query.Where(x => x.FamilyId == familyId && x.UserId == userId);
    }
}
