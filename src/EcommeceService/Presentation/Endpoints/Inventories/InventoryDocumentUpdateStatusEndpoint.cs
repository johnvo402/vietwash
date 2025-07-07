using Application.Common.Auth;
using Application.Feature.InventoryDocuments.Commands.UpdateStatus;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Inventories
{
    public class InventoryDocumentUpdateStatusEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<InventoryDocumentUpdateStatusCommand>.WithActionResult
    {
        [HttpPut(Router.InventoryRoute.UpdateStatus)]
        [SwaggerOperation(
            Tags = [Router.InventoryRoute.Tags],
            Summary = "Inventory Document update status"
        )]
        [AuthorizeBy]
        public override async Task<ActionResult> HandleAsync(
            InventoryDocumentUpdateStatusCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToCreatedResult();
        }
    }
}
