using Application.Features.Users.Commands.ResetPassword;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public class ResetUserPasswordEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ResetUserPasswordCommand>.WithActionResult
{
    [HttpPut(Router.UserRoute.ResetPassowrd)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "reset User password")]
    public override async Task<ActionResult> HandleAsync(
        ResetUserPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(request, cancellationToken);
        return this.NoContent204();
    }
}
