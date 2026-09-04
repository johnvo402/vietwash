using Application.Common.Auth;
using Application.Feature.Suppliers.Query.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
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
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<
            ActionResult<ApiResponse<GetSupplierDetailResponse>>
        > HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long supplierId,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(
                new GetSupplierDetailQuery(supplierId),
                cancellationToken
            );
            return result.ToActionResult();
        }
    }
}
