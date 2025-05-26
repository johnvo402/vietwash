using Application.Common.Auth;
using Application.Feature.Services.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Services
{
    public class DeleteServiceEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult
    {
        [HttpDelete(Router.ServiceRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Delete service")]
        //[AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.service}")]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long serviceId,
            CancellationToken cancellationToken = default
        )
        {
            await sender.Send(new DeleteServiceCommand(serviceId), cancellationToken);
            return NoContent();
        }
    }
}
