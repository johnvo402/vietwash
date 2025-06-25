using Application.Common.Auth;
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
        : EndpointBaseAsync.WithRequest<GetDashboardCardQuery>.WithActionResult<
            ApiResponse<GetDashboardCardResponse>
        >
    {
        [HttpGet(Presentation.Routes.Router.SaleResultRoute.DashboardCard)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SaleResultRoute.Tags],
            Summary = "Dashboard card"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<ActionResult<ApiResponse<GetDashboardCardResponse>>> HandleAsync(
            GetDashboardCardQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
