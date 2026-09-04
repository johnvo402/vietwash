using Application.Common.Auth;
using Application.Feature.InventoryDocuments.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Inventories
{
    public class DeleteInventoryEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<DeleteInventoryCommand>.WithActionResult<ApiResponse>
    {
        [HttpDelete(Router.InventoryRoute.GetUpdateDelete)]
        [SwaggerOperation(
            Tags = [Router.InventoryRoute.Tags],
            Summary = "Inventory Document delete"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            DeleteInventoryCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToNoContentResult();
        }
    }
}
