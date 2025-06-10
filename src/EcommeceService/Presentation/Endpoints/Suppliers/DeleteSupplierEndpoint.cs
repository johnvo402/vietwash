using Application.Common.Auth;
using Application.Feature.Services.Command.Delete;
using Application.Feature.Suppliers.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
    public class DeleteSupplierEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult
    {
        [HttpDelete(Router.SupplierRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Delete supplier")]
        //[AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.supplier}")]
        public override async Task<ActionResult> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long supplierId,
            CancellationToken cancellationToken = default
        )
        {
            await sender.Send(new DeleteSupplierCommand(supplierId), cancellationToken);
            return NoContent();
        }
    }
}
