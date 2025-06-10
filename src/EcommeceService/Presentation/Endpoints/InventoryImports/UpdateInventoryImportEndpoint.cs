using Application.Feature.InventoryImports.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.InventoryImports
{
    public class UpdateInventoryImportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateInventoryImportCommand>.WithActionResult<ApiResponse<Unit>>
    {
        [HttpPut(Router.InventoryImportRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.InventoryImportRoute.Tags], Summary = "update inventory import")]
        //[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.iventoryimport}")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
            UpdateInventoryImportCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return this.Created201();
        }
    }
}
