using Application.Common.Auth;
using Application.Feature.Units.Command.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Units
{
    public class DeleteUnitEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse>
    {
        [HttpDelete(Router.UnitRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.UnitRoute.Tags], Summary = "Delete Unit")]
        [AuthorizeBy(roles: "ADMIN, MANAGER")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromRoute(Name = RouterBase.Id)] long unitId,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(new DeleteUnitCommand(unitId), cancellationToken);
            return result.ToNoContentResult();
        }
    }
}
