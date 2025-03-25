using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetSaleStatisticEndpoint(ISender sender)
        : EndpointBaseAsync.WithoutRequest.WithActionResult<
            ApiResponse<IEnumerable<GetSaleResultResponse>>
        >
    {
        [HttpGet(Presentation.Routes.Router.SaleResultRoute.SaleResult)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SaleResultRoute.Tags],
            Summary = "statistic result"
        )]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetSaleResultResponse>>>
        > HandleAsync(CancellationToken cancellationToken = default) =>
            this.Ok200(await sender.Send(new GetSaleResultQuery(), cancellationToken));
    }
}
