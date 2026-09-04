using Application.Common.Auth;
using Application.Feature.Equipments.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Equipments
{
	public class GetEquipmentDetailEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<GetEquipmentDetailQuery>.WithActionResult<
			ApiResponse<GetEquipmentDetailResponse>
		>
	{
		[HttpGet(Routes.Router.EquipmentRoute.GetDetail)]
		[SwaggerOperation(Tags = [Routes.Router.EquipmentRoute.Tags], Summary = "Detail equipment")]
		[AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
		public override async Task<ActionResult<ApiResponse<GetEquipmentDetailResponse>>> HandleAsync(
			GetEquipmentDetailQuery request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
