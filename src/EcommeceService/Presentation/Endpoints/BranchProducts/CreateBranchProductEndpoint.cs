using Application.Common.Auth;
using Application.Feature.BranchProducts.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.BranchProducts
{
	public class CreateBranchProductEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<CreateBranchProductCommand>.WithActionResult<ApiResponse>
	{
		[HttpPost(Router.BranchProductRoute.BranchProducts)]
		[SwaggerOperation(Tags = [Router.BranchProductRoute.Tags], Summary = "Create branch product")]
		[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.branchproduct}")]
		public override async Task<ActionResult<ApiResponse>> HandleAsync(
			[FromBody] CreateBranchProductCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var user = await sender.Send(request, cancellationToken);
			return user.ToCreatedResult();
		}
	}
}
