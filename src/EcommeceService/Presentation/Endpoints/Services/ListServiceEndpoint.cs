using Application.Common.Auth;
using Application.Feature.Services.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services;

public class ListServiceEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListServiceQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListServiceResponse>>
    >
{
    [HttpGet(Router.ServiceRoute.Services)]
    [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Service list")]
    //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.service}")]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListServiceResponse>>>
    > HandleAsync(ListServiceQuery request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
