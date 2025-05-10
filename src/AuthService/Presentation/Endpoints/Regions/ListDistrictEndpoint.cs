using Application.Features.Regions.Queries.List.Districts;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Application.Features.Common.Projections.Regions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;

namespace Presentation.Endpoints.Regions;

public class ListDistrictEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListDistrictQuery>.WithActionResult<ApiResponse<PaginationResponse<DistrictDetailProjection>>>
{
    [HttpGet(Router.RegionRoute.Districts)]
    [SwaggerOperation(Tags = [Router.RegionRoute.Tags], Summary = "list District")]
    public override async Task<ActionResult<ApiResponse<PaginationResponse<DistrictDetailProjection>>>> HandleAsync(
        ListDistrictQuery request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
