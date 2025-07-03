using Application.Common.Auth;
using Application.Feature.InventoryDocuments.Commands.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Inventories
{
    public class CreateInventoryDocumentEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateInventoryDocumentCommand>.WithActionResult<
            ApiResponse<CreateInventoryDocumentResponse>
        >
    {
        [HttpPost(Router.InventoryRoute.Inventories)]
        [SwaggerOperation(
            Tags = [Router.OrderRoute.Tags],
            Summary = "Inventory Document a new order"
        )]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<CreateInventoryDocumentResponse>>
        > HandleAsync(
            [FromBody] CreateInventoryDocumentCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var order = await sender.Send(request, cancellationToken);
            return order.ToCreatedResult();
        }
    }
}
