using Application.Common.Auth;
using Application.Feature.Orders.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
	public class GetOrderDetailEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<string>.WithActionResult<ApiResponse<GetOrderDetailResponse>>
	{
		[HttpGet(Router.OrderRoute.GetUpdateDelete, Name = Router.OrderRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "Detail Order")]
		//[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.order}")]
		public override async Task<ActionResult<ApiResponse<GetOrderDetailResponse>>> HandleAsync(string request, CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}
