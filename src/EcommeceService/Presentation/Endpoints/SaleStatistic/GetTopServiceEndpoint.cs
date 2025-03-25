using Application.Feature.Statistics.Queries.TopService;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetTopServiceEndpoint(ISender sender)
        : EndpointBaseAsync.WithoutRequest.WithActionResult<
            ApiResponse<IEnumerable<GetTopServiceResponse>>
        >
    {
        [HttpGet(Presentation.Routes.Router.SaleResultRoute.TopService)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SaleResultRoute.Tags],
            Summary = "Get top services"
        )]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetTopServiceResponse>>>
        > HandleAsync(CancellationToken cancellationToken = default) =>
            this.Ok200(await sender.Send(new GetTopServiceQuery(), cancellationToken));
    }
}
