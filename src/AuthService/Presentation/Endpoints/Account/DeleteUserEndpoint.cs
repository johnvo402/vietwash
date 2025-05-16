using Application.Common.Auth;
using Application.Features.Accounts.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.Routers;

namespace Presentation.Endpoints.Account;

public class DeleteAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<long>.WithActionResult
{
    [HttpDelete(Router.AccountRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Delete Account")]
    [AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.user}")]
    public override async Task<ActionResult> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] long userId,
        CancellationToken cancellationToken = default
    )
    {
        await sender.Send(new DeleteAccountCommand(userId), cancellationToken);
        return this.NoContent204();
    }
}
