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
        : EndpointBaseAsync.WithRequest<string>.WithActionResult
    {
        [HttpDelete(Router.ServiceRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "Delete service")]
        [AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.service}")]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] string serviceId,
            CancellationToken cancellationToken = default
        )
        {
            await sender.Send(new DeleteServiceCommand(Ulid.Parse(serviceId)), cancellationToken);
            return NoContent();
        }
    }
}
