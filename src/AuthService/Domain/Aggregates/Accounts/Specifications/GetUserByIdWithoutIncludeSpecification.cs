using JohnChum.SharedKernel.Domain.Common.Specs;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByIdWithoutIncludeSpecification : Specification<Account>
{
    public GetAccountByIdWithoutIncludeSpecification(long id)
    {
        Query.Where(x => x.Id == id).AsNoTracking();
    }
}
