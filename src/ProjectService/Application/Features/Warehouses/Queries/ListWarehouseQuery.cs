using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Warehouses.Queries
{
    public class ListWarehouseQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListWarehouseResponse>>>;
}
