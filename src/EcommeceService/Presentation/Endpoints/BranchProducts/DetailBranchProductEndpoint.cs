using Application.Common.Auth;
using Application.Feature.BranchProducts.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.BranchProducts
{
    public class DetailBranchProductEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<DetailBranchProductQuery>.WithActionResult<
            ApiResponse<DetailBranchProductResponse>
        >
    {
        [HttpGet(Router.BranchProductRoute.GetUpdateDelete)]
        [SwaggerOperation(
            Tags = [Router.BranchProductRoute.Tags],
            Summary = "Detail branch product"
        )]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<DetailBranchProductResponse>>
        > HandleAsync(
            DetailBranchProductQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
