using Application.Common.Auth;
using Application.Features.Accounts.Commands.Create;
using Ardalis.ApiEndpoints;
using CaseConverter;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class CreateAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<CreateAccountCommand>.WithActionResult<ApiResponse<CreateAccountResponse>>
{
    [HttpPost(Router.AccountRoute.Accounts)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "create Account")]
    [AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.user}")]
    public override async Task<ActionResult<ApiResponse<CreateAccountResponse>>> HandleAsync(
        [FromForm] CreateAccountCommand request,
        CancellationToken cancellationToken = default
    )
    {
        CreateAccountResponse user = await sender.Send(request, cancellationToken);
        return this.Created201(Router.AccountRoute.GetRouteName, user.Id, user);
    }
}
