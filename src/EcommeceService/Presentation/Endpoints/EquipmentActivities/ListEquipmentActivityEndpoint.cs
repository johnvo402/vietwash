using Application.Common.Auth;
using Application.Feature.EquipmentActivities.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EquipmentActivities
{
	public class ListEquipmentActivityEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<ListEquipmentActivityQuery>.WithActionResult<
			ApiResponse<PaginationResponse<ListEquipmentActivityResponse>>
		>
	{
		[HttpGet(Router.EquipmentActivityRoute.EquipmentActivities)]
		[SwaggerOperation(Tags = [Router.EquipmentActivityRoute.Tags], Summary = "list  equipment activity")]
		[AuthorizeBy]
		public override async Task<
			ActionResult<ApiResponse<PaginationResponse<ListEquipmentActivityResponse>>>
		> HandleAsync(
			[FromQuery] ListEquipmentActivityQuery request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
