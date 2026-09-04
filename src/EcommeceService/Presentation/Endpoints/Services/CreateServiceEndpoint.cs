using Application.Common.Auth;
using Application.Feature.Services.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services
{
    public class CreateServiceEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateServiceCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.ServiceRoute.Services)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "create Service")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateServiceCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var user = await sender.Send(request, cancellationToken);
            return user.ToCreatedResult();
        }
    }
}
