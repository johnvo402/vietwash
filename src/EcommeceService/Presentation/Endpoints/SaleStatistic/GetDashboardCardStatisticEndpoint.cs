using Application.Common.Auth;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetSaleStatisticEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetDashboardCardQuery>.WithActionResult<
            ApiResponse<IEnumerable<GetDashboardCardResponse>>
        >
    {
        [HttpGet(Presentation.Routes.Router.SaleResultRoute.DashboardCard)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SaleResultRoute.Tags],
            Summary = "Dashboard card"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetDashboardCardResponse>>>
        > HandleAsync(
            [FromQuery] GetDashboardCardQuery request,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(request, cancellationToken));
    }
}
