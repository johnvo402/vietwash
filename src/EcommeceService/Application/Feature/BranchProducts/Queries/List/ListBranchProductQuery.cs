using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.BranchProducts.Queries.List
{
    public class ListBranchProductQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListBranchProductResponse>>>;
}
