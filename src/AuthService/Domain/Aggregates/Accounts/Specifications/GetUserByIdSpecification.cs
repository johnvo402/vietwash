using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByIdSpecification : Specification<Account>
{
    public GetAccountByIdSpecification(long id)
    {
        Query
            .Where(x => x.Id == id)
            .Include(x => x.AccountContacts)
            .Include(x => x.BranchAccounts)
            .AsSplitQuery();
    }
}
