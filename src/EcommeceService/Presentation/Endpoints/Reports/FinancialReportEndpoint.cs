using Application.Feature.Reports.FinancialReport;
using Ardalis.ApiEndpoints;
using Application.Common.Auth;
using Contracts.ApiWrapper; 
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.RouteResults;
using Mediator;
using Presentation.Routes;


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
