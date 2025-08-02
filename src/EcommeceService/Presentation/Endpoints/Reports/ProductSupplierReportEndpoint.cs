using Application.Feature.Reports.ProductSupplierReport;
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
    public class ProductSupplierReportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ProductSupplierReportQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ProductSupplierReportResponse>>
        >
    {
        [HttpGet(Router.ReportRoute.ProductSupplierReport)]
        [SwaggerOperation(Tags = [Router.ReportRoute.Tags], Summary = "Product supplier report")]
        // [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ProductSupplierReportResponse>>>
        > HandleAsync(
            [FromQuery] ProductSupplierReportQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
