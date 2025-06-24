using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Units.Queries.List
{
    public class ListUnitQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListUnitResponse>>>;
}
