using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Users.Specifications;

public class GetUserByIdIncludeResetPassword : Specification<User>
{
    public GetUserByIdIncludeResetPassword(long id)
    {
        Query.Where(x => x.Id == id).Include(x => x.UserResetPasswords).AsNoTracking();
    }
}
