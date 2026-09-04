using Application.Feature.Reports.CustomerRevenueReport;
using Ardalis.ApiEndpoints;
using Application.Common.Auth;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Domain.Functions;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Reports
{
    public class CustomerRevenueReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CustomerRevenueReportQuery>.WithActionResult<
            ApiResponse<PaginationResponse<CustomerRevenueResult>>
        >
    {
        [HttpGet(Router.ReportRoute.CustomerRevenue)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Customer revenue report")]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<CustomerRevenueResult>>>
        > HandleAsync(
            [FromQuery] CustomerRevenueReportQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
