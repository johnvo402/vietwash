using Application.Features.Branches.Queries;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Branches
{
    public class ListBranchEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListBranchQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListBranchResponse>>
        >
    {
        [HttpGet(Router.BranchRoute.Branches)]
        [SwaggerOperation(Tags = [Router.BranchRoute.Tags], Summary = "List Branch")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListBranchResponse>>>
        > HandleAsync(ListBranchQuery request, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
