using Application.Common.Auth;
using Application.Feature.Services.Command.Update;
using Application.Feature.Suppliers.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers 
{
	public class UpdateSupplierEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<UpdateSupplierCommand>.WithActionResult<
		ApiResponse<Mediator.Unit>
	>
	{
		[HttpPut(Router.SupplierRoute.GetUpdateDelete)]
		[SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Update supplier")]
		//[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.supplier}")]
		public override async Task<ActionResult<ApiResponse<Mediator.Unit>>> HandleAsync(
			UpdateSupplierCommand request,
			CancellationToken cancellationToken = default
		)
		{
			await sender.Send(request);
			return this.Created201();
		}
	}
}
