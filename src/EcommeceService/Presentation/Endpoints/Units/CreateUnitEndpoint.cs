using Application.Common.Auth;
using Application.Feature.Services.Command.Create;
using Application.Feature.Units.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using static Presentation.Routes.Router;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.RouteResults;

namespace Presentation.Endpoints.Units
{
	public class CreateUnitEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<CreateUnitCommand>.WithActionResult<ApiResponse<Unit>>
	{
		[HttpPost(UnitRoute.Units)]
		[SwaggerOperation(Tags = [UnitRoute.Tags], Summary = "Create a new unit")]
		//[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.user}")]
		public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
			[FromBody]CreateUnitCommand request, CancellationToken cancellationToken = default)
		{
			var unit = await sender.Send(request, cancellationToken);
			return this.Created201();
		}
	}
}
