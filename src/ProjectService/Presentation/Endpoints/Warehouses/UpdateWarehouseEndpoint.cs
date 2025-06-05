using Application.Features.Warehouses.Commands.Update;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Warehouses
{
    public class UpdateWarehouseEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<UpdateWarehouseCommand>.WithActionResult<ApiResponse<string>>
    {
        [HttpPut(Router.WarehouseRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "Update Warehouse")]
        public override async Task<ActionResult<ApiResponse<string>>> HandleAsync(UpdateWarehouseCommand request, CancellationToken cancellationToken = default)
        {
            var response = await sender.Send(request, cancellationToken);
            return this.Ok200(response);
        }
    }
}
