using Application.Common.Auth;
using Application.Feature.Feedbacks.Command.Reply;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
    public class CreateReplyFeedbackEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateReyplyFeedbackCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.FeedbackRoute.FeedbackReplies)]
        [SwaggerOperation(Tags = [Router.FeedbackRoute.Tags], Summary = "Reply feedback")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            CreateReyplyFeedbackCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var tariff = await sender.Send(request, cancellationToken);
            return tariff.ToCreatedResult();
        }
    }
}
