using Application.Feature.InventoryImports.Command.Update;
using Application.Feature.InventoryImports.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.InventoryImports
{
    public class ListInventoryImportEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListInventoryImportQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListInventoryImportResponse>>
    >
    {
        [HttpGet(Router.InventoryImportRoute.InventoryImports)]
        [SwaggerOperation(Tags = [Router.InventoryImportRoute.Tags], Summary = "InventoryImport list")]
        //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.iventoryimport}")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListInventoryImportResponse>>>
        > HandleAsync(ListInventoryImportQuery request, CancellationToken cancellationToken = default)
        {
            try
            {
                return this.Ok200(await sender.Send(request, cancellationToken));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while processing ListInventoryImportEndpoint.HandleAsync");
                return StatusCode(500, "lỗi");
            }
        }
    }
}
