using Application.Feature.Reports.CustomerRevenueReport;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Domain.Functions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
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
        // [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<CustomerRevenueResult>>>
        > HandleAsync(
            [FromQuery] CustomerRevenueReportQuery request,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(request, cancellationToken));
    }
}
