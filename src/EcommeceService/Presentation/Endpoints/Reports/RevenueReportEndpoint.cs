using Application.Feature.Reports.RevenueReport;
using Ardalis.ApiEndpoints;
using Application.Common.Auth;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Reports
{
    public class RevenueReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<RevenueReportQuery>.WithActionResult<
            ApiResponse<PaginationResponse<RevenueReportResponse>>
        >
    {
        [HttpGet(Router.ReportRoute.RevenueReport)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Report reveune")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<RevenueReportResponse>>>
        > HandleAsync(
            [FromQuery] RevenueReportQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
