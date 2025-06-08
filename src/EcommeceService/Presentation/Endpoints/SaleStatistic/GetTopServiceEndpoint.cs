using Application.Common.Auth;
using Application.Feature.Statistics.Queries.TopService;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.SaleStatistic
{
    public class GetTopServiceEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetTopServiceQuery>.WithActionResult<
            ApiResponse<IEnumerable<GetTopServiceResponse>>
        >
    {
        [HttpGet(Presentation.Routes.Router.SaleResultRoute.TopService)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SaleResultRoute.Tags],
            Summary = "Top Service"
        )]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<IEnumerable<GetTopServiceResponse>>>
        > HandleAsync(
            [FromQuery] GetTopServiceQuery request,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(request, cancellationToken));
    }
}
