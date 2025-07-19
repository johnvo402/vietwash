using Application.Common.Auth;
using Application.Feature.EquipmentActivities.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EquipmentActivities
{
	public class GetEquipmentActivityEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<GetEquipmentActivityDetailQuery>.WithActionResult<
			ApiResponse<GetEquipmentActivityDetailResponse>
		>
	{
		[HttpGet(Routes.Router.EquipmentActivityRoute.GetDetail)]
		[SwaggerOperation(Tags = [Routes.Router.EquipmentActivityRoute.Tags], Summary = "Detail equipment activity")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse<GetEquipmentActivityDetailResponse>>> HandleAsync(
			GetEquipmentActivityDetailQuery request,
			CancellationToken cancellationToken = default
		)
		{	
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
