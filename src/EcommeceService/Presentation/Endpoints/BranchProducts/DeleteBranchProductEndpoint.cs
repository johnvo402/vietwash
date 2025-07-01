using Application.Common.Auth;
using Application.Feature.BranchProducts.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.BranchProducts
{
	public class DeleteBranchProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<DeleteBranchProductCommand>.WithActionResult<ApiResponse>
	{
		[HttpDelete(Router.BranchProductRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.BranchProductRoute.Tags], Summary = "Delete branch product")]
		[AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.branchproduct}")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			DeleteBranchProductCommand request,
			CancellationToken cancellationToken = default
		)
		{
			await sender.Send(request);
			return NoContent();
		}
	}
}
