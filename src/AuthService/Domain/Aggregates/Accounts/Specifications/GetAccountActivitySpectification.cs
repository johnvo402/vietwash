using Domain.Aggregates.Accounts.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;


namespace Domain.Aggregates.Accounts.Specifications
{
    public class GetAccountActivitySpecification : Specification<AccountActivity>
    {
        public GetAccountActivitySpecification(long accountId, AccountActivityType type)
        {
            Query.Where(x =>x.AccountId == accountId && x.Type == type)
                .AsNoTracking();
        }
    }
}
