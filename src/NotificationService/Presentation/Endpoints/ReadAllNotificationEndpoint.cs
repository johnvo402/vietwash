using Application.Common.Auth;
using Application.Features.Notifications.Commands.ReadAllNotify;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints
{
    public class ReadAllNotificationEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ReadAllNotifyCommand>.WithActionResult
    {
        [HttpPut(Router.ReadAllNotify)]
        [SwaggerOperation(Tags = [Router.Tags], Summary = "list Notify")]
        [AuthorizeBy]
        public override async Task<ActionResult> HandleAsync(
            ReadAllNotifyCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
