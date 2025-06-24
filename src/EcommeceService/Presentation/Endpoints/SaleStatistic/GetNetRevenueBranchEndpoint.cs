using Application.Common.Auth;
using Application.Feature.Statistics.Queries.BranchNetRevenue;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetNetRevenueBranchEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetNetRevenueBranchQuery>.WithActionResult<
            ApiResponse<IEnumerable<GetNetRevenueBranchResponse>>
        >
    {
        [HttpGet(Routes.Router.SaleResultRoute.NetRevenueBranch)]
        [SwaggerOperation(
            Tags = [Routes.Router.SaleResultRoute.Tags],
            Summary = "Get net revenue branch by date"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetNetRevenueBranchResponse>>>
        > HandleAsync(
            [FromQuery] GetNetRevenueBranchQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
