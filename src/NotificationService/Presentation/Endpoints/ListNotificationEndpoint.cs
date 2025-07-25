using Application.Common.Auth;
using Application.Features.Notifications.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints
{
    public class ListNotificationEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListNotificationQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListNotificationResponse>>
        >
    {
        [HttpGet(Router.ListNotify)]
        [SwaggerOperation(Tags = [Router.Tags], Summary = "list Notify")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListNotificationResponse>>>
        > HandleAsync(
            [FromQuery] ListNotificationQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
