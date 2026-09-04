using Application.Common.Auth;
using Application.Feature.BranchProducts.Command.Update;
using Application.Feature.BranchProducts.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.BranchProducts
{
    public class ListBranchProductEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListBranchProductQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListBranchProductResponse>>
        >
    {
        [HttpGet(Router.BranchProductRoute.BranchProducts)]
        [SwaggerOperation(Tags = [Router.BranchProductRoute.Tags], Summary = "list branch product")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListBranchProductResponse>>>
        > HandleAsync(
            [FromQuery] ListBranchProductQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
