using Application.Common.Auth;
using Application.Feature.Products.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Products
{
	public class GetProductDetailEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<GetProductDetailQuery>.WithActionResult<
			ApiResponse<GetProductDetailResponse>
		>
	{
		[HttpGet(Presentation.Routes.Router.ProductRoute.GetDetail)]
		[SwaggerOperation(
			Tags = [Presentation.Routes.Router.ProductRoute.Tags],
			Summary = "Detail Product"
		)]
		[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.product}")]
		public override async Task<
			ActionResult<ApiResponse<GetProductDetailResponse>>
		> HandleAsync(
			GetProductDetailQuery query,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(query);
			return result.ToActionResult();
		}
	}
}
