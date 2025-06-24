using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByIdIncludeResetPassword : Specification<Account>
{
    public GetAccountByIdIncludeResetPassword(long id)
    {
        Query.Where(x => x.Id == id).Include(x => x.AccountResetPasswords).AsNoTracking();
    }
}
