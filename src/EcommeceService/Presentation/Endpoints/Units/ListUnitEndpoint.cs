using Application.Common.Auth;
using Application.Feature.Units.Queries.List;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Units
{
	public class ListUnitEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<ListUnitQuery>.WithActionResult<ApiResponse<PaginationResponse<ListUnitResponse>>>
	{
		[HttpGet(Router.UnitRoute.Units)]
		[SwaggerOperation(Tags = [Router.UnitRoute.Tags], Summary = "list Unit")]
		//[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.unit}")]
		public override async Task<ActionResult<ApiResponse<PaginationResponse<ListUnitResponse>>>> HandleAsync(
			ListUnitQuery request, 
			CancellationToken cancellationToken = default
		) => this.Ok200(await sender.Send(request, cancellationToken));
	}
}
