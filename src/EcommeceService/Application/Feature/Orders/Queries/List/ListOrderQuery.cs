using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;


namespace Application.Feature.Orders.Queries.List
{
	public class ListOrderQuery : QueryParamRequest, IRequest<PaginationResponse<ListOrderResponse>>;
}
