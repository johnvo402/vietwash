using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;


namespace Application.Feature.Products.Queries.List
{
	public class ListProductQuery : QueryParamRequest,
		IRequest<Result<PaginationResponse<ListProductResponse>>>;
}
