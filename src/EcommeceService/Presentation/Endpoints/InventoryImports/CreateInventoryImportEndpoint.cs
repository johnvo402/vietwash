using Application.Common.Auth;
using Application.Feature.InventoryImports.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.InventoryImports
{
    public class CreateInventoryImportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateInventoryImportCommand>.WithActionResult<ApiResponse<Unit>>
    {
        [HttpPost(Router.InventoryImportRoute.InventoryImports)]
        [SwaggerOperation(Tags = [Router.InventoryImportRoute.Tags], Summary = "create inventory import")]
        //[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.iventoryimport}")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
            [FromBody] CreateInventoryImportCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return this.Created201();
        }
    }
}
