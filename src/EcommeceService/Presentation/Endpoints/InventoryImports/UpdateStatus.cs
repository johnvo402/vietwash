using Application.Common.Auth;
using Application.Feature.InventoryImports.Command.UpdateStautus;
using Application.Feature.Orders.Command.UpdateStatus;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.InventoryImports
{
    public class UpdateStatus(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateStatusInventoryImportCommand>.WithActionResult<ApiResponse<UpdateStatusInventoryImportResponse>>
    {
        [HttpPut(Router.InventoryImportRoute.UpdateStatus)]
        [SwaggerOperation(Tags = [Router.InventoryImportRoute.Tags], Summary = "Update Status InventoryImports")]
        //[AuthorizeBy(permissions: $"{ActionPermission.update}:{ObjectPermission.iventoryimport}")]
        public async override Task<ActionResult<ApiResponse<UpdateStatusInventoryImportResponse>>> HandleAsync(UpdateStatusInventoryImportCommand request, CancellationToken cancellationToken = default)
        {
            var response = await sender.Send(request, cancellationToken);
            return this.Ok200(response);
        }
    }
}
