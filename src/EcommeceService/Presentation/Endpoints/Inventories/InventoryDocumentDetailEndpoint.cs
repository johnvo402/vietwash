using Application.Common.Auth;
using Application.Feature.InventoryDocuments.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Inventories
{
    public class InventoryDocumentDetailEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<InventoryDocumentDetailQuery>.WithActionResult<
            ApiResponse<InventoryDocumentDetailResponse>
        >
    {
        [HttpGet(Router.InventoryRoute.GetUpdateDelete)]
        [SwaggerOperation(
            Tags = [Router.InventoryRoute.Tags],
            Summary = "Inventory Document detail"
        )]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<InventoryDocumentDetailResponse>>
        > HandleAsync(
            InventoryDocumentDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToCreatedResult();
        }
    }
}
