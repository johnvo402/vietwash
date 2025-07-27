using Application.Common.Auth;
using Application.Feature.BranchProducts.Queries.ListCardInv;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.BranchProducts
{
    public class BranchProductCardInventoryEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<BranchProductCardInventoryQuery>.WithActionResult<
            ApiResponse<PaginationResponse<BranchProductCardInventoryResponse>>
        >
    {
        [HttpGet(Router.BranchProductRoute.BranchProductCardInv)]
        [SwaggerOperation(Tags = [Router.BranchProductRoute.Tags], Summary = "list branch product")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<BranchProductCardInventoryResponse>>>
        > HandleAsync(
            [FromQuery] BranchProductCardInventoryQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
