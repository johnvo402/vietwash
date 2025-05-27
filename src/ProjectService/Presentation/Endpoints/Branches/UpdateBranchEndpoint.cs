using Application.Features.Branches.Branch.Commands.Update;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class UpdateBranchEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<UpdateBranchCommand>.WithActionResult<ApiResponse<UpdateBranchResponse>>
    {
        [HttpPut(Router.BranchRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "Update Branch")]
        public override async Task<ActionResult<ApiResponse<UpdateBranchResponse>>> HandleAsync([FromBody]UpdateBranchCommand request, CancellationToken cancellationToken = default)
        {
            var response = await sender.Send(request, cancellationToken);
            return this.Ok200(response);
        }
    }
}
