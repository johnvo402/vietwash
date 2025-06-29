using Application.Common.Auth;
using Application.Feature.Products.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Products
{
	public class ListProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<ListProductQuery>.WithActionResult<
			ApiResponse<PaginationResponse<ListProductResponse>>
		>
	{
		[HttpGet(Router.ProductRoute.Products)]
		[SwaggerOperation(Tags = [Router.ProductRoute.Tags], Summary = "Product list")]
		[AuthorizeBy]
		public override async Task<
			ActionResult<ApiResponse<PaginationResponse<ListProductResponse>>>
		> HandleAsync(
			[FromQuery] ListProductQuery request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request, cancellationToken);
			return result.ToActionResult();
		}
	}
}
