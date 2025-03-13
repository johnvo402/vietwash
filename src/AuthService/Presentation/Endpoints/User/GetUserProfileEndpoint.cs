using Application.Common.Auth;
using Application.Features.Users.Queries.Profiles;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public class GetUserProfileEndpoint(ISender sender)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<ApiResponse<GetUserProfileResponse>>
{
    [HttpGet(Router.UserRoute.Profile)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "Profile User")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse<GetUserProfileResponse>>> HandleAsync(
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(new GetUserProfileQuery(), cancellationToken));
}
