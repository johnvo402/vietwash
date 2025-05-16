using JohnChum.SharedKernel.Domain.Common.Specs;
using Microsoft.EntityFrameworkCore;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByEmailSpecification : Specification<Account>
{
    public GetAccountByEmailSpecification(string email)
    {
        Query.Where(x => EF.Functions.ILike(x.Email, email) && !x.Disabled).AsNoTracking();
    }
}
