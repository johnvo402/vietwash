using Application.Common.Auth;
using Application.Feature.Orders.Queries.Detail;
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
	public class GetOrderDetailEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<GetOrderDetailQuery>.WithActionResult<ApiResponse<GetOrderDetailResponse>>
	{
		[HttpGet(Router.OrderRoute.GetUpdateDelete, Name = Router.OrderRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Detail Order")]
		[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.order}")]
		public override async Task<ActionResult<ApiResponse<GetOrderDetailResponse>>> HandleAsync(
			GetOrderDetailQuery request, 
			CancellationToken cancellationToken = default
		)
		{
			var response = await sender.Send(new GetOrderDetailQuery { OrderId = request.OrderId }, cancellationToken);
			return this.Ok200(response);
		}
	}
}
