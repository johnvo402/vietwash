using System.Linq.Expressions;
using Domain.Aggregates.Accounts;

namespace Application.Features.Customers.Queries.ListCustomer
{
    public class ListCustomerMapping
    {
        public static Expression<Func<Account, ListCustomerResponse>> Selector()
        {
            return account => new ListCustomerResponse
            {
                Id = account.Id,
                PublicId = account.PublicId,
                CreatedAt = account.CreatedAt,
                CreatedBy = account.CreatedBy,
                UpdatedAt = account.UpdatedAt,
                UpdatedBy = account.UpdatedBy,
                CustomerGroup = account.CustomerGroup,
                DisplayName = account.DisplayName,
                PhoneNumber = account.PhoneNumber,
                AvtUrl = account.AvtUrl,

                Status = account.Status,
            };
        }
    }
}
