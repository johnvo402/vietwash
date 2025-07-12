using Application.Common.Auth;
using Application.Feature.Feedbacks.Command.React;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
    public class ReactFeedbackEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ReactFeedbackCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.FeedbackRoute.React)]
        [SwaggerOperation(Tags = [Router.FeedbackRoute.Tags], Summary = "React to feedback")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            ReactFeedbackCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
