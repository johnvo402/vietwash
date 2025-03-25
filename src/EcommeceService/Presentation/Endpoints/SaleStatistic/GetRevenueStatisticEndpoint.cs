using Application.Feature.Statistics.Queries.RevenueStatistic;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetRevenueStatisticEndpoint(ISender sender)
        : EndpointBaseAsync.WithoutRequest.WithActionResult<
            ApiResponse<IEnumerable<GetRevenueStatisticResponse>>
        >
    {
        [HttpGet(Presentation.Routes.Router.SaleResultRoute.RevenueStatistic)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SaleResultRoute.Tags],
            Summary = "Get revenue statistics by date"
        )]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetRevenueStatisticResponse>>>
        > HandleAsync(CancellationToken cancellationToken = default) =>
            this.Ok200(await sender.Send(new GetRevenueStatisticQuery(), cancellationToken));
    }
}
