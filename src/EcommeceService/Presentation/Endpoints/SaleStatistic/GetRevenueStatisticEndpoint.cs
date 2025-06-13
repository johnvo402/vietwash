using Application.Common.Auth;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetRevenueStatisticEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetRevenueStatisticQuery>.WithActionResult<
            ApiResponse<IEnumerable<GetRevenueStatisticResponse>>
        >
    {
        [HttpGet(Routes.Router.SaleResultRoute.RevenueStatistic)]
        [SwaggerOperation(
            Tags = [Routes.Router.SaleResultRoute.Tags],
            Summary = "Get revenue statistics by date"
        )]
        // [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetRevenueStatisticResponse>>>
        > HandleAsync(
            [FromQuery] GetRevenueStatisticQuery request,
            CancellationToken cancellationToken = default
        )
        {
            return this.Ok200(await sender.Send(request, cancellationToken));
        }
    }
}
