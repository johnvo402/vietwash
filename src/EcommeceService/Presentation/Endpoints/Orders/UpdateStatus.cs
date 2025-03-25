using Application.Common.Auth;
using Application.Feature.Orders.Command.Update;
using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Services.Command.Update;
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
	public class UpdateStatus(ISender sender)
	: EndpointBaseAsync.WithRequest<UpdateStatusCommand>.WithActionResult<ApiResponse<UpdateStatusResponse>>
	{
		[HttpPut(Router.OrderRoute.UpdateStatus)]
		[SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Update Status Order")]
		[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.order}")]
		public async override Task<ActionResult<ApiResponse<UpdateStatusResponse>>> HandleAsync(UpdateStatusCommand request, CancellationToken cancellationToken = default)
		{
			var response = await sender.Send(request, cancellationToken);
			return this.Ok200(response);
		}
	}
}
