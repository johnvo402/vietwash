using JohnChum.SharedKernel.Domain.Common.Specs;
using Microsoft.EntityFrameworkCore;
using Nest;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByPhoneNumberSpecification : Specification<Account>
{
    public GetAccountByPhoneNumberSpecification(string phone)
    {
        Query
            .Where(x =>
                x.PhoneNumber == phone && !x.Disabled && x.Role == "CUSTOMER"
            )
            .AsNoTracking();
    }
}
