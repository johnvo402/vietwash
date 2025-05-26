using Application.Features.Branches.Branch.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class DeleteBranchEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<long>.WithActionResult
    {
        [HttpDelete(Router.BranchRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "Delete Branch")]
        public override async Task<ActionResult> HandleAsync([FromRoute(Name = RouterBase.Id)] long branchId, CancellationToken cancellationToken = default)
        {
            await sender.Send(new DeleteBranchCommand(branchId), cancellationToken);
            return this.NoContent204();
        }
    }
}
