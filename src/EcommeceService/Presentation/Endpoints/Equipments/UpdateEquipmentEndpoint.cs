using Application.Common.Auth;
using Application.Feature.Equipments.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Equipments
{
	public class UpdateEquipmentEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<UpdateEquipmentCommand>.WithActionResult<ApiResponse>
	{
		[HttpPut(Router.EquipmentRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.EquipmentRoute.Tags], Summary = "Update equipment")]
		[AuthorizeBy]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateEquipmentCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request);
			return result.ToActionResult();
		}
	}
}
