using Application.Common.Auth;
using Application.Feature.Feedbacks.Queries.ListByService;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Feedbacks
{
	public class ListFeedbackByServiceEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<ListFeedbackByServiceQuery>.WithActionResult<
			ApiResponse<IEnumerable<ListFeedbackByServiceResponse>>
		>
	{
		[HttpGet(Router.ServiceRoute.Feedbacks)]
		[SwaggerOperation(Tags = [Router.ServiceRoute.Tags], Summary = "List feedback from service")]
		[AuthorizeBy(roles: ROLE.CUSTOMER)]
		public override async Task<
			ActionResult<ApiResponse<IEnumerable<ListFeedbackByServiceResponse>>>
		> HandleAsync(ListFeedbackByServiceQuery request, CancellationToken cancellationToken = default)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
