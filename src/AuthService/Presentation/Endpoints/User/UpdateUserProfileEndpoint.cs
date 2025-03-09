using Application.Common.Auth;
using Application.Features.Users.Commands.Profiles;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public class UpdateUserProfileEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateUserProfileCommand>.WithActionResult<ApiResponse>
{
    [HttpPut(Router.UserRoute.Profile)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "Update Profile User")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        [FromForm] UpdateUserProfileCommand request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
