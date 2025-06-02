using Application.Common.Auth;
using Application.Feature.Services.Queries.List;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;
using Serilog;

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
        try
        {
           return this.Ok200(await sender.Send(request, cancellationToken));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while processing ListServiceEndpoint.HandleAsync");
            return StatusCode(500, "lỗi");
        }
    }
}
