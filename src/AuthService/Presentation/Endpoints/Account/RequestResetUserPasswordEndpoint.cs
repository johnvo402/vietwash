using Application.Features.Accounts.Commands.RequestResetPassword;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class RequestResetAccountPasswordEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<RequestResetAccountPasswordCommand>.WithoutResult
{
    [HttpPut(Router.AccountRoute.RequestResetPassowrd)]
    [AllowAnonymous]
    [SwaggerOperation(
        Tags = [Router.AccountRoute.Tags],
        Summary = "request reset Account password"
    )]
    public override async Task<IActionResult> HandleAsync(
        RequestResetAccountPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToNoContentResult();
    }
}
