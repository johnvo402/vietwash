using Application.Feature.Suppliers.Query.Detail;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Contracts.Routers;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Suppliers
{
    public class GetSupplierDetailEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<
            ApiResponse<GetSupplierDetailResponse>
        >
    {
        [HttpGet(Presentation.Routes.Router.SupplierRoute.GetDetail)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.SupplierRoute.Tags],
            Summary = "Detail supplier"
        )]
        //[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.supplier}")]
        public override async Task<ActionResult<ApiResponse<GetSupplierDetailResponse>>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long supplierId,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(new GetSupplierDetailQuery(supplierId), cancellationToken));
    }
}
