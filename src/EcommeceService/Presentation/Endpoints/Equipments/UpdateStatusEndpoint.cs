using Application.Common.Auth;
using Application.Feature.Equipments.Command.UpdateStatus;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Equipments
{
	public class UpdateStatusEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<UpdateStatusEquipmentCommand>.WithActionResult<ApiResponse>
	{
		[HttpPut(Router.EquipmentRoute.UpdateStatus)]
		[SwaggerOperation(Tags = [Router.EquipmentRoute.Tags], Summary = "Update Status Equipment")]
		[AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateStatusEquipmentCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var response = await sender.Send(request, cancellationToken);
			return response.ToActionResult();
		}
	}
}
