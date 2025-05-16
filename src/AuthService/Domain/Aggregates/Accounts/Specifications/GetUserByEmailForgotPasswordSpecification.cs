using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetUserByEmailForgotPasswordSpecification : Specification<Account>
{
    public GetUserByEmailForgotPasswordSpecification(string email)
    {
        Query.Where(x => x.Email == email).Include(x => x.AccountResetPasswords).AsNoTracking();
    }
}
