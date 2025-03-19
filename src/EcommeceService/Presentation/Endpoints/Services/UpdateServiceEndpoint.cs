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
    : EndpointBaseAsync.WithRequest<UpdateServiceCommand>.WithActionResult<
        ApiResponse<Mediator.Unit>
    >
{
    [HttpPut(Router.ServiceRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Update service")]
    public override async Task<ActionResult<ApiResponse<Mediator.Unit>>> HandleAsync(
        [FromForm] UpdateServiceCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(request);
        return this.Created201();
    }
}
