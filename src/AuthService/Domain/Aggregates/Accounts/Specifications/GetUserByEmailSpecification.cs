using Microsoft.EntityFrameworkCore;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByEmailSpecification : Specification<Account>
{
    public GetAccountByEmailSpecification(string email)
    {
        Query
            .Where(x => EF.Functions.ILike(x.Email, email) && !x.Disabled)
            .Include(x => x.BranchAccounts)
            .AsNoTracking();
    }
}
