using Application.Features.Warehouses.Commands.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Warehouses
{
    public class UpdateWarehouseEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<UpdateWarehouseCommand>.WithActionResult<ApiResponse<UpdateWarehouseResponse>>
    {
        [HttpPut(Router.WarehouseRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "Update Warehouse")]
        public override async Task<ActionResult<ApiResponse<UpdateWarehouseResponse>>> HandleAsync([FromBody] UpdateWarehouseCommand request, CancellationToken cancellationToken = default)
        {
            var response = await sender.Send(request, cancellationToken);
            return this.Ok200(response);
        }
    }
}
