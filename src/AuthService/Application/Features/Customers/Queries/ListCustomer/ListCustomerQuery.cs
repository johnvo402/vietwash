

using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Customers.Queries.ListCustomer
{
    public class ListCustomerQuery : QueryParamRequest, IRequest<PaginationResponse<ListCustomerResponse>>;
}
