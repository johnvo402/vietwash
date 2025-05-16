using Application.Features.Accounts.Commands.ResetPassword;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class ResetAccountPasswordEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ResetAccountPasswordCommand>.WithActionResult
{
    [HttpPut(Router.AccountRoute.ResetPassowrd)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "reset Account password")]
    public override async Task<ActionResult> HandleAsync(
        ResetAccountPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(request, cancellationToken);
        return this.NoContent204();
    }
}
