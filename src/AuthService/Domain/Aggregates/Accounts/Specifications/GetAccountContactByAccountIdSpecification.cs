using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications
{
    public class GetAccountContactByAccountIdSpecification : Specification<AccountContact>
    {
        public GetAccountContactByAccountIdSpecification(long accountId)
        {
            Query.Where(x => x.AccountId == accountId).Include(x => x.Account).AsSplitQuery();
        }
    }
}
