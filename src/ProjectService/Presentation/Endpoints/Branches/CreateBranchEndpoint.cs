using Application.Features.Branches.Commands.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class CreateBranchEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateBranchCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.BranchRoute.Branches)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "Create Branch")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateBranchCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var branch = await sender.Send(request, cancellationToken);
            return branch.ToCreatedResult();
        }
    }
}
