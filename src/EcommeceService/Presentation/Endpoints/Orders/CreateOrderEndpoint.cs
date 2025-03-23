using Application.Common.Auth;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Units.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
	public class CreateOrderEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<CreateOrderCommand>.WithActionResult<ApiResponse<Unit>>
	{
		[HttpPost(Router.OrderRoute.Orders)]
		[SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Create a new order")]
		//[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.order}")]
		public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
			[FromBody]CreateOrderCommand request, 
			CancellationToken cancellationToken = default)
		{
		
			var order = await sender.Send(request, cancellationToken);
			return this.Created201();
		}
	}
}
