using Domain.Functions;
using Application.Feature.Reports.OrderReport;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Reports
{
    public class OrderReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<OrderReportQuery>.WithActionResult<
            ApiResponse<PaginationResponse<OrderSummaryResult>>
        >
    {
        [HttpGet(Router.ReportRoute.Order)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Report service order")]
        // [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<OrderSummaryResult>>>
        > HandleAsync(
            [FromQuery] OrderReportQuery request,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(request, cancellationToken));
    }
}
