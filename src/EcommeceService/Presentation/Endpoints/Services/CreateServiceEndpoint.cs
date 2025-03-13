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
    : EndpointBaseAsync.WithRequest<CreateServiceCommand>.WithActionResult<ApiBaseResponse>
    {
        [HttpPost(Router.ServiceRoute.Services)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "create Service")]
        //[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.user}")]
        public override async Task<ActionResult<ApiBaseResponse>> HandleAsync(
            [FromBody]CreateServiceCommand request, CancellationToken cancellationToken = default)
        {
            CreateServiceResponse user = await sender.Send(request, cancellationToken);
            return this.NoContent204();
        }
    }
}
