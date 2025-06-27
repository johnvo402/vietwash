using Application.Common.Auth;
using Application.Feature.Products.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Products
{
	public class DeleteProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<DeleteProductCommand>.WithActionResult<ApiResponse>
	{
		[HttpDelete(Router.ProductRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.ProductRoute.Tags], Summary = "Delete product")]
		[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.product}")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			DeleteProductCommand request,
			CancellationToken cancellationToken = default
		)
		{
			await sender.Send(request);
			return NoContent();
		}
	}
}
