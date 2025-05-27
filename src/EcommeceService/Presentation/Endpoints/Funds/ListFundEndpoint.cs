using Application.Common.Auth;
using Application.Feature.Funds.Queries.List;
using Application.Feature.Orders.Queries.List;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Funds
{
	public class ListFundEndpoint(ISender sender)
	: EndpointBaseAsync.WithRequest<ListFundQuery>.WithActionResult<ApiResponse<PaginationResponse<ListFundResponse>>>
	{
		[HttpGet(Router.FundRoute.Funds)]
		[SwaggerOperation(Tags = [Router.FundRoute.Tags], Summary = "list Fund")]
		[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.fund}")]
		public override async Task<ActionResult<ApiResponse<PaginationResponse<ListFundResponse>>>> HandleAsync(
		[FromQuery]	ListFundQuery request,
			CancellationToken cancellationToken = default
		) => this.Ok200(await sender.Send(request, cancellationToken));
	}
}
