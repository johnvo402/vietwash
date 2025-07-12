using Application.Common.Auth;
using Application.Feature.Feedbacks.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
    public class UpdateFeedbackEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateFeedbackCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.FeedbackRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.FeedbackRoute.Tags], Summary = "Update feedback")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateFeedbackCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
