using Application.Common.Auth;
using Application.Feature.Products.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Products
{
	public class UpdateProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<UpdateProductCommand>.WithActionResult<ApiResponse>
	{
		[HttpPut(Router.ProductRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.ProductRoute.Tags], Summary = "Update product")]
		//[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.product}")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			UpdateProductCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var result = await sender.Send(request);
			return result.ToActionResult();
		}
	}
}
