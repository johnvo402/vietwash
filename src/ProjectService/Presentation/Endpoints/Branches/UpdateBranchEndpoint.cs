using Application.Features.Branches.Commands.Update;
using Application.Common.Auth;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class UpdateBranchEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateBranchCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.BranchRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "Update Branch")]
        [AuthorizeBy(roles: "ADMIN")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateBranchCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var response = await sender.Send(request, cancellationToken);
            return response.ToActionResult();
        }
    }
}
