using Application.Common.Auth;
using Application.Features.Accounts.Commands.ChangePassword;
using Ardalis.ApiEndpoints;
using Ardalis.Result.AspNetCore;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class ChangeAccountPasswordEnpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ChangeAccountPasswordCommand>.WithActionResult<ApiResponse>
{
    [HttpPut(Router.AccountRoute.ChangePassword)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "reset Account password")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        ChangeAccountPasswordCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
