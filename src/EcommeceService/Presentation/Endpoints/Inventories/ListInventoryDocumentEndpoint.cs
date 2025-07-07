using Application.Common.Auth;
using Application.Feature.InventoryDocuments.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Inventories
{
    public class ListInventoryDocumentEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListInventoryDocumentQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListInventoryDocumentResponse>>
        >
    {
        [HttpGet(Router.InventoryRoute.Inventories)]
        [SwaggerOperation(Tags = [Router.InventoryRoute.Tags], Summary = "Inventory Document list")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListInventoryDocumentResponse>>>
        > HandleAsync(
            [FromQuery] ListInventoryDocumentQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToCreatedResult();
        }
    }
}
