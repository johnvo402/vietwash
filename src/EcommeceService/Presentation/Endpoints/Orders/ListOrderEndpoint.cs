using Application.Common.Auth;
using Application.Feature.Orders.Queries.List;
using Application.Feature.Units.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders
{
    public class ListOrderEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListOrderQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListOrderResponse>>
        >
    {
        [HttpGet(Router.OrderRoute.Orders)]
        [SwaggerOperation(Tags = [Router.OrderRoute.Tags], Summary = "list Order")]
        //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.order}")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListOrderResponse>>>
        > HandleAsync(
            [FromQuery] ListOrderQuery request,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(request, cancellationToken));
    }
}
