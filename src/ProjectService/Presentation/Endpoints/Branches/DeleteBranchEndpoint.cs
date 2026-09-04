using Application.Features.Branches.Commands.Delete;
using Application.Common.Auth;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class DeleteBranchEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse>
    {
        [HttpDelete(Router.BranchRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "Delete Branch")]
        [AuthorizeBy(roles: "ADMIN")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long branchId,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(new DeleteBranchCommand(branchId), cancellationToken);
            return result.ToNoContentResult();
        }
    }
}
