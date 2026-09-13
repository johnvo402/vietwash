using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByEmailSpecification : Specification<Account>
{
    public GetAccountByEmailSpecification(string email)
    {
        string normalizedEmail = email.ToLowerInvariant();
        Query
            .Where(x => x.Email != null && x.Email.ToLower() == normalizedEmail && !x.Disabled)
            .Include(x => x.BranchAccounts)
            .AsNoTracking();
    }
}
