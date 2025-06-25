using Application.Feature.Products.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Products
{
	public class DeleteProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse>
	{
		[HttpDelete(Router.ProductRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.ProductRoute.Tags], Summary = "Delete product")]
		//[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.product}")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			[FromRoute(Name = RouterBase.Id)] long productId,
			CancellationToken cancellationToken = default
		)
		{
			await sender.Send(new DeleteProductCommand(productId), cancellationToken);
			return NoContent();
		}
	}
}
