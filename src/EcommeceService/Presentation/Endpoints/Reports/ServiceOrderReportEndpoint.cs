using Application.Common.Auth;
using Application.Feature.Services.Queries.List;
using Application.Feature.Services.Queries.ServiceOrderReport;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Reports
{
    public class ServiceOrderReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ServiceRevenueReportQuery>.WithActionResult<
            ApiResponse<List<ServiceRevenueReportResponse>>
        >
    {
        [HttpGet(Router.ReportRoute.ReportServiceOrder)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Report service order")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<List<ServiceRevenueReportResponse>>>
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
