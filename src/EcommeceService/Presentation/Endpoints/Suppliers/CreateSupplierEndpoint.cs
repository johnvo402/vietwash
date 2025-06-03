using Application.Feature.Suppliers.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
	public class CreateSupplierEndpoint(ISender sender)
		: EndpointBaseAsync.WithRequest<CreateSupplierCommand>.WithActionResult<ApiResponse<Unit>>
	{
		[HttpPost(Router.SupplierRoute.Suppliers)]
		[SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "create Supplier")]
		//[AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.supplier}")]
		public override async Task<ActionResult<ApiResponse<Unit>>> HandleAsync(
			[FromBody] CreateSupplierCommand request,
			CancellationToken cancellationToken = default
		)
		{
			var user = await sender.Send(request, cancellationToken);
			return this.Created201();
		}
	}
}
