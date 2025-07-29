using Domain.Aggregates.Accounts;

namespace Application.Features.Customers.Queries.Detail
{
    public static class GetCustomerDetailMapping
    {
        public static GetCustomerDetailResponse ToGetCustomerDetailResponse(this Account account)
        {
            var response = new GetCustomerDetailResponse();
            response.MappingFrom(account);
            response.CustomerGroup = account.CustomerGroup;
            return response;
        }
    }
}
