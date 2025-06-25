using Application.Features.Warehouses.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Warehouses
{
    public class ListWarehouseEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListWarehouseQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListWarehouseResponse>>
        >
    {
        [HttpGet(Router.WarehouseRoute.Warehouses)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "List Branch Product")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListWarehouseResponse>>>
        > HandleAsync(
            [FromQuery] ListWarehouseQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
