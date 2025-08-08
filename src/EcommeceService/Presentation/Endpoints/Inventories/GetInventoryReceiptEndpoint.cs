using Application.Feature.InventoryDocuments.Queries.GetReceipt;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Inventories
{
    public class GetInventoryReceiptEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<InventoryReceiptQuery>.WithActionResult<
            ApiResponse<InventoryReceiptResponse>
        >
    {
        [HttpGet(Router.InventoryRoute.GetReceipt)]
        [SwaggerOperation(
            Tags = [Router.InventoryRoute.Tags],
            Summary = "Inventory Document GetReceipt"
        )]
        // [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<InventoryReceiptResponse>>> HandleAsync(
            InventoryReceiptQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
