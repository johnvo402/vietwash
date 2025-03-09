using Application.Common.Auth;
using Application.Features.Roles.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.Routers;

namespace Presentation.Endpoints.Roles;

public class DeleteRoleEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<string>.WithActionResult<ApiResponse>
{
    [HttpDelete(Router.RoleRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.RoleRoute.Tags], Summary = "Delete Role")]
    [AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.role}")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] string roleId,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteRoleCommand(Ulid.Parse(roleId)), cancellationToken);
        return this.NoContent204();
    }
}
