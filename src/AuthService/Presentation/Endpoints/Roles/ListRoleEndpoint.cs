using Application.Common.Auth;
using Application.Features.Roles.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Roles;

public class ListRoleEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListRoleQuery>.WithActionResult<ApiResponse<IEnumerable<ListRoleResponse>>>
{
    [HttpGet(Router.RoleRoute.Roles)]
    [SwaggerOperation(Tags = [Router.RoleRoute.Tags], Summary = "List Role")]
    [AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.role}")]
    public override async Task<ActionResult<ApiResponse<IEnumerable<ListRoleResponse>>>> HandleAsync(
        [FromQuery] ListRoleQuery request,
        CancellationToken cancellationToken = default
    ) => new ApiResponse<IEnumerable<ListRoleResponse>>(await sender.Send(request, cancellationToken), Message.SUCCESS);
}
