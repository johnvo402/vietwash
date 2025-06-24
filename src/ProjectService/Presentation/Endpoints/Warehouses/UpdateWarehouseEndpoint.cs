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
    public class UpdateWarehouseEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateWarehouseCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.WarehouseRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "Update Warehouse")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateWarehouseCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
