using Application.Common.Auth;
using Application.Feature.EquipmentActivities.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.EquipmentActivities
{
	public class CreateEquipmentActivityEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<CreateEquipmentActivityCommand>.WithActionResult<ApiResponse>
	{
		[HttpPost(Router.EquipmentActivityRoute.EquipmentActivities)]
		[SwaggerOperation(Tags = [Router.EquipmentActivityRoute.Tags], Summary = "create quipment activities")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			[FromBody] CreateEquipmentActivityCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var user = await sender.Send(request, cancellationToken);
			return user.ToCreatedResult();
		}
	}
}
