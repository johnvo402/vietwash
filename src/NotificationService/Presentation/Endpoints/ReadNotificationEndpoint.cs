using Application.Common.Auth;
using Application.Features.Notifications.Commands.ReadNotify;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints
{
    public class ReadNotificationEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ReadNotifyCommand>.WithActionResult
    {
        [HttpPatch(Router.ReadOneNotify)]
        [SwaggerOperation(Tags = [Router.Tags], Summary = "list Notify")]
        [AuthorizeBy]
        public override async Task<ActionResult> HandleAsync(
            ReadNotifyCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
