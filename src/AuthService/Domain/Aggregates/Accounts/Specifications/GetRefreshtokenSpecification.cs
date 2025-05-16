using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetRefreshtokenSpecification : Specification<AccountToken>
{
    public GetRefreshtokenSpecification(string token, long userId)
    {
        Query.Where(x => x.AccountId == userId && x.Token == token).Include(x => x.Account);
    }
}
