using Application.Common.Auth;
using Application.Feature.Services.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services;

public class UpdateServiceEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateServiceCommand>.WithActionResult<ApiResponse>
{
    [HttpPut(Router.ServiceRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Update service")]
    [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        UpdateServiceCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request);
        return result.ToActionResult();
    }
}
