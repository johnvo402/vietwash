using JohnChum.SharedKernel.Domain.Common.Specs;
using Microsoft.EntityFrameworkCore;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByPhoneNumberSpecification : Specification<Account>
{
    public GetAccountByPhoneNumberSpecification(string phone, string role)
    {
        Query.Where(x => x.PhoneNumber == phone && !x.Disabled && x.Role == role).AsNoTracking();
    }
}
