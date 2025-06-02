using Application.Common.Auth;
using Application.Feature.Suppliers.Query.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
	public class ListSupplierEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<ListSupplierQuery>.WithActionResult<
		ApiResponse<PaginationResponse<ListSupplierResponse>>
	>
	{
		[HttpGet(Router.SupplierRoute.Suppliers)]
		[SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Supplier list")]
		//[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.supplier}")]
		public override async Task<
			ActionResult<ApiResponse<PaginationResponse<ListSupplierResponse>>>
		> HandleAsync(ListSupplierQuery request, CancellationToken cancellationToken = default) =>
			this.Ok200(await sender.Send(request, cancellationToken));
	}
}
