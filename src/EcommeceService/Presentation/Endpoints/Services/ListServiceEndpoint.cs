using Application.Feature.Services.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services;

public class ListServiceEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListServiceQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListServiceResponse>>
    >
{
    [HttpGet(Router.ServiceRoute.Services)]
    [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Service list")]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListServiceResponse>>>
    > HandleAsync(ListServiceQuery request, CancellationToken cancellationToken = default) =>
        this.Ok200(await sender.Send(request, cancellationToken));
}
