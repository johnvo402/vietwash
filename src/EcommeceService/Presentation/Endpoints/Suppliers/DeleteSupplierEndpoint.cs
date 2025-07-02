using Application.Common.Auth;
using Application.Feature.Suppliers.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
    public class DeleteSupplierEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse>
    {
        [HttpDelete(Router.SupplierRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Delete supplier")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long supplierId,
            CancellationToken cancellationToken = default
        )
        {
            await sender.Send(new DeleteSupplierCommand(supplierId), cancellationToken);
            return NoContent();
        }
    }
}
