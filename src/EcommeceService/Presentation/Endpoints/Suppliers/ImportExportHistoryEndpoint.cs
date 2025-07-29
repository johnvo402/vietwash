using Application.Common.Auth;
using Application.Feature.Suppliers.Query.ImportExportHistory;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
    public class ImportExportHistoryEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ImportExportHistoryQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ImportExportHistoryResponse>>
        >
    {
        [HttpGet(Router.SupplierRoute.ImportExportHistories)]
        [SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Supplier list")]
        [AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ImportExportHistoryResponse>>>
        > HandleAsync(
            [FromQuery] ImportExportHistoryQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
