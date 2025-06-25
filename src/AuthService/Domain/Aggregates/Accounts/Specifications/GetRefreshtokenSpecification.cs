using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetRefreshtokenSpecification : Specification<AccountToken>
{
    public GetRefreshtokenSpecification(string token, long userId)
    {
        Query.Where(x => x.AccountId == userId && x.Token == token);
    }
}
