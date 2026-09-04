using Application.Common.Auth;
using Application.Feature.Units.Command.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Presentation.Routes.Router;

namespace Presentation.Endpoints.Units
{
    public class CreateUnitEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateUnitCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(UnitRoute.Units)]
        [SwaggerOperation(Tags = [UnitRoute.Tags], Summary = "Create a new unit")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateUnitCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var unit = await sender.Send(request, cancellationToken);
            return unit.ToCreatedResult();
        }
    }
}
