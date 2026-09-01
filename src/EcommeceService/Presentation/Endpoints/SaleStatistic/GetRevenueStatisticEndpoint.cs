using Application.Feature.Statistics.Queries.RevenueStatistic;
using Ardalis.ApiEndpoints;
using Application.Common.Auth;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Domain.Functions;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetRevenueStatisticEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetRevenueStatisticQuery>.WithActionResult<
            ApiResponse<IEnumerable<GetRevenueStatistic>>
        >
    {
        [HttpGet(Routes.Router.SaleResultRoute.RevenueStatistic)]
        [SwaggerOperation(
            Tags = [Routes.Router.SaleResultRoute.Tags],
            Summary = "Get revenue statistics by date"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetRevenueStatistic>>>
        > HandleAsync(
            [FromQuery] GetRevenueStatisticQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
