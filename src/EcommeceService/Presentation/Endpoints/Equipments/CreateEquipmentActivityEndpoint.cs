using Application.Common.Auth;
using Application.Feature.Equipments.Command.CreateActivities;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Equipments
{
	public class CreateEquipmentActivityEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<CreateEquipmentActivityCommand>.WithActionResult<ApiResponse>
	{
		[HttpPost(Router.EquipmentRoute.Activities)]
		[SwaggerOperation(Tags = [Router.EquipmentRoute.Tags], Summary = "Create activities for equipment")]
		[AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			CreateEquipmentActivityCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var tariff = await sender.Send(request, cancellationToken);
			return tariff.ToCreatedResult();
		}
	}
}
