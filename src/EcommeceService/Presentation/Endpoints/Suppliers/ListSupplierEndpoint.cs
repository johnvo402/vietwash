using Application.Common.Auth;
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
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListSupplierResponse>>>
        > HandleAsync(
            [FromQuery] ListSupplierQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
