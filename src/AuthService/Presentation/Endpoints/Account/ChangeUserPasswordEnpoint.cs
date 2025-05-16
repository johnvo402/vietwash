using Application.Common.Auth;
using Application.Features.Accounts.Commands.ChangePassword;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class ChangeAccountPasswordEnpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ChangeAccountPasswordCommand>.WithActionResult
{
    [HttpPut(Router.AccountRoute.ChangePassword)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "reset Account password")]
    [AuthorizeBy]
    public override async Task<ActionResult> HandleAsync(
        ChangeAccountPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(request, cancellationToken);
        return this.NoContent204();
    }
}
