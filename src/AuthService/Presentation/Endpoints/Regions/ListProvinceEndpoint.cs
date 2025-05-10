using Application.Features.Regions.Queries.List.Provinces;
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

public class ListProvinceEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListProvinceQuery>.WithActionResult<ApiResponse<PaginationResponse<ProvinceProjection>>>
{
    [HttpGet(Router.RegionRoute.Provinces)]
    [SwaggerOperation(Tags = [Router.RegionRoute.Tags], Summary = "list Province")]
    public override async Task<ActionResult<ApiResponse<PaginationResponse<ProvinceProjection>>>> HandleAsync(
        ListProvinceQuery request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
