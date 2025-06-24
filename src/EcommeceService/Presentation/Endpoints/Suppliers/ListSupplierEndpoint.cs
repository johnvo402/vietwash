using Application.Feature.Suppliers.Query.List;
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
    public class ListSupplierEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListSupplierQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListSupplierResponse>>
        >
    {
        [HttpGet(Router.SupplierRoute.Suppliers)]
        [SwaggerOperation(Tags = [Router.SupplierRoute.Tags], Summary = "Supplier list")]
        //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.supplier}")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListSupplierResponse>>>
        > HandleAsync(ListSupplierQuery request, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
