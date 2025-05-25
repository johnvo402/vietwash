using Application.Features.Branches.Branch.Commands.Delete;
using Application.Features.Warehouses.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Warehouses
{
    public class DeleteWarehouseEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<long>.WithActionResult
    {
        [HttpDelete(Router.WarehouseRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.WarehouseRoute.Tags], Summary = "Delete Warehouse")]
        public override async Task<ActionResult> HandleAsync([FromRoute(Name = RouterBase.Id)] long warehouseId, CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteWarehouseCommand(warehouseId), cancellationToken);
            return this.NoContent204();
        }
    }
}
