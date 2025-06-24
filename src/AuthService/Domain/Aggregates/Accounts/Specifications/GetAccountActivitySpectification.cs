using Domain.Aggregates.Accounts.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications
{
    public class GetAccountActivitySpecification : Specification<AccountActivity>
    {
        public GetAccountActivitySpecification(long accountId, AccountActivityType type, string ip)
        {
            Query
                .Where(x => x.AccountId == accountId && x.Type == type && x.Ip == ip)
                .AsNoTracking();
        }
    }
}
