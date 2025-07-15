using Application.Common.Auth;
using Application.Feature.EquipmentActivities.Command.UpdateStatus;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EquipmentActivities
{
	public class UpdateStatusEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<UpdateStatusEquipmentActivityCommand>.WithActionResult<ApiResponse>
	{
		[HttpPut(Router.EquipmentActivityRoute.UpdateStatus)]
		[SwaggerOperation(Tags = [Router.EquipmentActivityRoute.Tags], Summary = "Update Status EquipmentActivity")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateStatusEquipmentActivityCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var response = await sender.Send(request, cancellationToken);
			return response.ToActionResult();
		}
	}
}
