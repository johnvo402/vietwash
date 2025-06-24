using Microsoft.EntityFrameworkCore;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications;

public class GetAccountByPhoneNumberSpecification : Specification<Account>
{
    public GetAccountByPhoneNumberSpecification(string phone, string role)
    {
        Query.Where(x => x.PhoneNumber == phone && !x.Disabled && x.Role == role).AsNoTracking();
    }
}
