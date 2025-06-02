using Application.Common.Auth;
using Application.Features.Accounts.Commands.Update;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class UpdateAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateAccountCommand>.WithActionResult<ApiResponse<UpdateAccountResponse>>
{
    [HttpPut(Router.AccountRoute.GetUpdateDelete)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Update Account")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse<UpdateAccountResponse>>> HandleAsync(
        UpdateAccountCommand command,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(command, cancellationToken));
}
