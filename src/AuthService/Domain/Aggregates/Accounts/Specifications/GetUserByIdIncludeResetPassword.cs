using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByIdIncludeResetPassword : Specification<Account>
{
    public GetAccountByIdIncludeResetPassword(long id)
    {
        Query.Where(x => x.Id == id).Include(x => x.AccountResetPasswords).AsNoTracking();
    }
}
