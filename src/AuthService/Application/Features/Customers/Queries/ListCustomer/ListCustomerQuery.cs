using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Customers.Queries.ListCustomer
{
    public class ListCustomerQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListCustomerResponse>>>;
}
