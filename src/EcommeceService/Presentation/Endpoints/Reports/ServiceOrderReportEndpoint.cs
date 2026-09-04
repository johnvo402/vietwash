using Application.Common.Auth;
using Application.Feature.Reports.Queries.ServiceOrderReport;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Reports
{
    public class ServiceOrderReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ServiceRevenueReportQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ServiceRevenueReportResponse>>
        >
    {
        [HttpGet(Router.ReportRoute.ReportServiceOrder)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Report service order")]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ServiceRevenueReportResponse>>>
        > HandleAsync(
            [FromQuery] ServiceRevenueReportQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
