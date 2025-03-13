using Application.Features.Users.Commands.RequestResetPassword;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public class RequestResetUserPasswordEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<RequestResetUserPasswordCommand>.WithActionResult
{
    [HttpPut(Router.UserRoute.RequestResetPassowrd)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "request reset User password")]
    public override async Task<ActionResult> HandleAsync(
        RequestResetUserPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(request, cancellationToken);
        return this.NoContent204();
    }
}
