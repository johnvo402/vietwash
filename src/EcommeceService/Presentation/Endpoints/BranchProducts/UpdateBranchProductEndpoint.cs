using Application.Common.Auth;
using Application.Feature.BranchProducts.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.BranchProducts
{
    public class UpdateBranchProductEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateBranchProductCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.BranchProductRoute.GetUpdateDelete)]
        [SwaggerOperation(
            Tags = [Router.BranchProductRoute.Tags],
            Summary = "Update branch product"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateBranchProductCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
