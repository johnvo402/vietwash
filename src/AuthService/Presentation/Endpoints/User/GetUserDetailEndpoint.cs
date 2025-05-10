using Application.Common.Auth;
using Application.Features.Users.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.Routers;

namespace Presentation.Endpoints.User;

public class GetUserDetailEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<string>.WithActionResult<ApiResponse<GetUserDetailResponse>>
{
    [HttpGet(Router.UserRoute.GetUpdateDelete, Name = Router.UserRoute.GetRouteName)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "Detail User")]
    [AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.user}")]
    public override async Task<ActionResult<ApiResponse<GetUserDetailResponse>>> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] string userId,
        CancellationToken cancellationToken = default
    ) =>
        this.Ok200(
            await sender.Send(new GetUserDetailQuery(Ulid.Parse(userId)), cancellationToken)
        );
}
