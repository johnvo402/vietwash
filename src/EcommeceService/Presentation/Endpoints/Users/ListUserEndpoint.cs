using System.Threading;
using Application.Common.Auth;
using Application.Features.Users.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public class ListUserEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListUserQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListUserResponse>>
    >
{
    [HttpGet(Router.UserRoute.Users)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "list User")]
    [AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.user}")]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListUserResponse>>>
    > HandleAsync([FromQuery] ListUserQuery request, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
