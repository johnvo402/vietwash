using Application.Common.Auth;
using Application.Features.Users.Commands.Logout;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User
{
    public class LogoutEndpoint(ISender sender)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<ApiResponse<LogoutResponse>>
    {
        [HttpPost(Router.UserRoute.Logout)]
        [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "Logout in User")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<LogoutResponse>>> HandleAsync(
            CancellationToken cancellationToken = default
        )
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var command = new LogoutCommand { Token = token };
           return this.Ok200(await sender.Send(command, cancellationToken));
        }
    }
}
