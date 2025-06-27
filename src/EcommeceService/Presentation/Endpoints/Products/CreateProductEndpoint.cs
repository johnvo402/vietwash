using Application.Feature.Products.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.RouteResults;
using Application.Common.Auth;
using Infrastructure.Constants;


namespace Presentation.Endpoints.Products
{
	public class CreateProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<CreateProductCommand>.WithActionResult<ApiResponse>
	{
		[HttpPost(Router.ProductRoute.Products)]
		[SwaggerOperation(Tags = [Router.ProductRoute.Tags], Summary = "Create product")]
		[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.product}")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			[FromBody] CreateProductCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var product = await sender.Send(request, cancellationToken);
			return product.ToCreatedResult();
		}
	}
}
