using Application.Common.Auth;
using Application.Feature.EquipmentActivities.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EquipmentActivities
{
	public class UpdateEquipmentActivityEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<UpdateEquipmentActivityCommand>.WithActionResult<ApiResponse>
	{
		[HttpPut(Router.EquipmentActivityRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.EquipmentActivityRoute.Tags], Summary = "Update equipment activity")]
		[AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateEquipmentActivityCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request);
			return result.ToActionResult();
		}
	}
}
