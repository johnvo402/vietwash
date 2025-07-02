using Application.Common.Auth;
using Application.Feature.Suppliers.Command.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
    public class UpdateSupplierEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateSupplierCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.SupplierRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Update supplier")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateSupplierCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
