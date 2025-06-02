using Application.Features.Warehouses.Queries;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Warehouses
{
    public class ListWarehouseEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<ListWarehouseQuery>.WithActionResult<ApiResponse<PaginationResponse<ListWarehouseResponse>>>
    {
        [HttpGet(Router.WarehouseRoute.Warehouses)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "List Branch Product")]
        public override async Task<ActionResult<ApiResponse<PaginationResponse<ListWarehouseResponse>>>> HandleAsync(ListWarehouseQuery request, CancellationToken cancellationToken = default)
        {
            return this.Ok200(await sender.Send(request, cancellationToken));
        }
    }
}
