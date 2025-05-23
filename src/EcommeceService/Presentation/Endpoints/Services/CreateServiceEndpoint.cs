using Application.Common.Auth;
using Application.Feature.Services.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services
{
    public class CreateServiceEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateServiceCommand>.WithActionResult<ApiResponse<Unit>>
    {
        [HttpPost(Router.ServiceRoute.Services)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "create Service")]
        //[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.service}")]
        public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
            [FromBody] CreateServiceCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var user = await sender.Send(request, cancellationToken);
            return this.Created201();
        }
    }
}
