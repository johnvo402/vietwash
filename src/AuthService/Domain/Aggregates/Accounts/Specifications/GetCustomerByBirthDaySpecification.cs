using Domain.Aggregates.Accounts.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Accounts.Specifications
{
    public class GetCustomerByBirthDaySpecification : Specification<Account>
    {
        public GetCustomerByBirthDaySpecification(DateOnly birthDay)
        {
            Query
                .Where(x =>
                    x.Role == "CUSTOMER"
                    && x.BirthDay.Day == birthDay.Day
                    && x.BirthDay.Month == birthDay.Month
                    && x.CustomerGroup == CustomerGroup.Loyal
                    && x.Email != null
                )
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
