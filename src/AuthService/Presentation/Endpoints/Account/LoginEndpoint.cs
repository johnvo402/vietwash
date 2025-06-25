using Application.Features.Accounts.Commands.Login;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class LoginEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<LoginCommand>.WithActionResult<ApiResponse<LoginResponse>>
{
    [HttpPost(Router.AccountRoute.Login)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Logging in Account")]
    public override async Task<ActionResult<ApiResponse<LoginResponse>>> HandleAsync(
        LoginCommand request,
        CancellationToken cancellationToken = default
    ) => (await sender.Send(request, cancellationToken)).ToActionResult();
}
