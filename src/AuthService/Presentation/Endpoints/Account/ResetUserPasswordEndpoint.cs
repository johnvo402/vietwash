using Application.Features.Accounts.Commands.ResetPassword;
using Ardalis.ApiEndpoints;
using Ardalis.Result.AspNetCore;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class ResetAccountPasswordEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ResetAccountPasswordCommand>.WithoutResult
{
    [HttpPut(Router.AccountRoute.ResetPassowrd)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "reset Account password")]
    public override async Task<IActionResult> HandleAsync(
        ResetAccountPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
