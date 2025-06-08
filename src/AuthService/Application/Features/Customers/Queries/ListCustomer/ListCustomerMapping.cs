

using AutoMapper;
using Domain.Aggregates.Accounts;
using System.Linq.Expressions;

namespace Application.Features.Customers.Queries.ListCustomer
{
    public class ListCustomerMapping :Profile
    {
        public ListCustomerMapping()
        {
            CreateMap<Account, ListCustomerResponse>();
        }

    }
}
