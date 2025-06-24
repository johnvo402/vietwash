using Application.Common.Auth;
using Application.Features.Accounts.Commands.Delete;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Contracts.Routers;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class DeleteAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse>
{
    [HttpDelete(Router.AccountRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Delete Account")]
    [AuthorizeBy(permissions: $"{ActionPermission.delete}:{ObjectPermission.user}")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] long userId,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new DeleteAccountCommand(userId), cancellationToken);

        return result.ToNoContentResult();
    }
}
