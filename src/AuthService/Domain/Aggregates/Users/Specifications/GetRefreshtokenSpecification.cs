using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Users.Specifications;

public class GetRefreshtokenSpecification : Specification<UserToken>
{
    public GetRefreshtokenSpecification(string token, long userId)
    {
        Query.Where(x => x.UserId == userId && x.RefreshToken == token).Include(x => x.User);
    }
}
