using Application.Common.Auth;
using Application.Feature.Equipments.Queries.List;
using Application.Feature.Equipments.Queries.Listl;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Equipments
{
	public class ListEquipmentEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<ListEquipmentQuery>.WithActionResult<
			ApiResponse<PaginationResponse<ListEquipmentResponse>>
		>
	{
		[HttpGet(Router.EquipmentRoute.Equipments)]
		[SwaggerOperation(Tags = [Router.EquipmentRoute.Tags], Summary = "Equipment list")]
		[AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
		public override async Task<
			ActionResult<ApiResponse<PaginationResponse<ListEquipmentResponse>>>
		> HandleAsync(
			[FromQuery] ListEquipmentQuery request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
