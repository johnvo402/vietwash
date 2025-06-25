using Application.Common.Auth;
using Application.Feature.Orders.Queries.List;
using Application.Feature.Units.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
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
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListOrderResponse>>>
        > HandleAsync(
            [FromQuery] ListOrderQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
