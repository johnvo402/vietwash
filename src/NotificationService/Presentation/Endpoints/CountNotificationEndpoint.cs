using Application.Common.Auth;
using Application.Features.Notifications.Queries.CountNotiUnRead;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints
{
    public class CountNotificationEndpoint(ISender sender)
        : EndpointBaseAsync.WithoutRequest.WithActionResult<ApiResponse<CountNotifyUnReadResponse>>
    {
        [HttpGet(Router.CountNotify)]
        [SwaggerOperation(Tags = [Router.Tags], Summary = "list Notify")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<CountNotifyUnReadResponse>>
        > HandleAsync(CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(new CountNotifyUnReadQuery(), cancellationToken);
            return result.ToActionResult();
        }
    }
}
