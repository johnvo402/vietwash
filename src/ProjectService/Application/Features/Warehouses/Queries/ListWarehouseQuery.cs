using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Warehouses.Queries
{
    public class ListWarehouseQuery : QueryParamRequest, IRequest<PaginationResponse<ListWarehouseResponse>>
    {
    }
}
