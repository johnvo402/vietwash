using Application.Common.Auth;
using Application.Feature.Feedbacks.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
	public class DeleteFeedbackEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<DeleteFeedbackCommand>.WithActionResult<ApiResponse>
	{
		[HttpDelete(Router.FeedbackRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.FeedbackRoute.Tags], Summary = "Delete feedback")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			DeleteFeedbackCommand request,
			CancellationToken cancellationToken = default
		)
		{
			await sender.Send(request);
			return NoContent();
		}
	}
}
