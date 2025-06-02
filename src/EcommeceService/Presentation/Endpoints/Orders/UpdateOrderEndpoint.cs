using Application.Common.Auth;
using Application.Feature.Orders.Command.Create;
using Application.Feature.Orders.Command.Update;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
	public class UpdateOrderEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<UpdateOrderCommand>.WithActionResult<ApiResponse<UpdateOrderResponse>>
	{
		[HttpPut(Router.OrderRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Update Order")]
		[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.order}")]
		public override async Task<ActionResult<ApiResponse<UpdateOrderResponse>>> HandleAsync(
			UpdateOrderCommand command,
			CancellationToken cancellationToken = default
		) => this.Ok200(await sender.Send(command, cancellationToken));
		
	}
}
