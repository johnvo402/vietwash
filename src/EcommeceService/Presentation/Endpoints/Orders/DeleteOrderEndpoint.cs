using Application.Common.Auth;
using Application.Feature.Units.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Contracts.Routers;
using Domain.Aggregates.Services;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
	public class DeleteOrderEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<string>.WithActionResult
	{
		[HttpDelete(Router.OrderRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Delete Order")]
		//[AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.order}")]
		public override async Task<ActionResult> HandleAsync(
		[FromRoute(Name = RouterBase.Id)]string orderId, 
			CancellationToken cancellationToken = default
		)
		{
			await sender.Send(new DeleteUnitCommand(Ulid.Parse(orderId)), cancellationToken);
			return this.NoContent204();
		}
	}
}
