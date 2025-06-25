using Application.Common.Auth;
using Application.Feature.Units.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Units
{
    public class ListUnitEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListUnitQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListUnitResponse>>
        >
    {
        [HttpGet(Router.UnitRoute.Units)]
        [SwaggerOperation(Tags = [Router.UnitRoute.Tags], Summary = "list Unit")]
        //[AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.unit}")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListUnitResponse>>>
        > HandleAsync(
            [FromQuery] ListUnitQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
