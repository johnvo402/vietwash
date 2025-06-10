using Application.Common.Auth;
using Application.Feature.InventoryImports.Queries.Detail;
using Application.Feature.Services.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Contracts.Routers;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.InventoryImports
{
    public class GetInventoryImportEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<
            ApiResponse<GetInventoryImportDetailResponse>
        >
    {
        [HttpGet(Presentation.Routes.Router.InventoryImportRoute.GetDetail)]
        [SwaggerOperation(
            Tags = [Presentation.Routes.Router.InventoryImportRoute.Tags],
            Summary = "Detail InventoryImport"
        )]
        //[AuthorizeBy(permissions: $"{ActionPermission.detail}:{ObjectPermission.iventoryimport}")]
        public override async Task<ActionResult<ApiResponse<GetInventoryImportDetailResponse>>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long inventoryImportId,
            CancellationToken cancellationToken = default
        ) => this.Ok200(await sender.Send(new GetInventoryImportDetailQuery(inventoryImportId), cancellationToken));
    }
}
