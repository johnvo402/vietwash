using Application.Features.Warehouses.Commands.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Warehouses
{
    public class CreateWarehouseEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<CreateWarehouseCommand>.WithActionResult<ApiResponse<Unit>>
    {
        [HttpPost(Router.WarehouseRoute.Warehouses)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "Create Warehouse")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync([FromBody] CreateWarehouseCommand request, CancellationToken cancellationToken = default)
        {
            var warehouse = await sender.Send(request, cancellationToken);
            return this.Created201();
        }
    }
}
