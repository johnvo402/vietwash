using Application.Common.Auth;
using Application.Features.Accounts.Commands.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class UpdateAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateAccountCommand>.WithActionResult<
        ApiResponse<UpdateAccountResponse>
    >
{
    [HttpPut(Router.AccountRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Update Account")]
    [AuthorizeBy(roles: "ADMIN, MANAGER")]
    public override async Task<ActionResult<ApiResponse<UpdateAccountResponse>>> HandleAsync(
        UpdateAccountCommand command,
        CancellationToken cancellationToken = default
    ) => (await sender.Send(command, cancellationToken)).ToActionResult();
}
