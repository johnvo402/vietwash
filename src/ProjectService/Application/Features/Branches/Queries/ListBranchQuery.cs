using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Branches.Queries
{
    public class ListBranchQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListBranchResponse>>>;
}
