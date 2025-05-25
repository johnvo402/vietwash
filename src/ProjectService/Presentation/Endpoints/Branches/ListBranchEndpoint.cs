using Application.Features.Branches.Branch.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class ListBranchEndpoint(ISender sender) : EndpointBaseAsync.WithRequest<ListBranchQuery>.WithActionResult<ApiResponse<PaginationResponse<ListBranchResponse>>>
    {
        [HttpGet(Router.BranchRoute.Branches)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "List Branch")]
        public override async Task<ActionResult<ApiResponse<PaginationResponse<ListBranchResponse>>>> HandleAsync(ListBranchQuery request, CancellationToken cancellationToken = default)
        {
            return this.Ok200(await sender.Send(request, cancellationToken));
        }

    }
}
