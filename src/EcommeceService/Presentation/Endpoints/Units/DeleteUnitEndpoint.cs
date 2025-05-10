using Application.Common.Auth;
using Application.Feature.Units.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Units
{
	public class DeleteUnitEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<long>.WithActionResult
	{
		[HttpDelete(Router.UnitRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.UnitRoute.Tags], Summary = "Delete Unit")]
		//[AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.unit}")]
		public override async Task<ActionResult> HandleAsync(
			[FromRoute(Name = RouterBase.Id)] long unitId, CancellationToken cancellationToken = default)
		{
			await sender.Send(new DeleteUnitCommand(unitId), cancellationToken);
			return this.NoContent204();
		}
	}
}
