using Application.Common.Auth;
using Application.Feature.Statistics.Queries.BranchNetRevenue;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
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
            return this.Ok200(await sender.Send(request, cancellationToken));
        }
    }
}
