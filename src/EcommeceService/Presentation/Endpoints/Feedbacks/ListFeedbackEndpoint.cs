using Application.Common.Auth;
using Application.Feature.Feedbacks.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
	public class ListFeedbackEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<ListFeedbackQuery>.WithActionResult<
		ApiResponse<PaginationResponse<ListFeedbackResponse>>
	>
	{
		[HttpGet(Router.FeedbackRoute.Feedbacks)]
		[SwaggerOperation(Tags = [Router.FeedbackRoute.Tags], Summary = "Feedback list")]
		[AuthorizeBy]
		public override async Task<
			ActionResult<ApiResponse<PaginationResponse<ListFeedbackResponse>>>
		> HandleAsync(
			[FromQuery] ListFeedbackQuery request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
