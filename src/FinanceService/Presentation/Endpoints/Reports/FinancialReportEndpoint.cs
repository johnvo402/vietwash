using Application.Features.Report.FinanceReport;
using Application.Common.Auth;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.RouteResults;

namespace Presentation.Endpoints.Reports
{
    public class FinancialReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<FinancialReportQuery>.WithActionResult<
            ApiResponse<FinancialReportResponse>
        >
    {
        [HttpGet(Router.ReportRoute.FinancialReport)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Report financial")]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<FinancialReportResponse>>
        > HandleAsync(
            [FromQuery] FinancialReportQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
