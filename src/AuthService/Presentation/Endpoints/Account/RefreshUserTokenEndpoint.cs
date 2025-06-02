using Application.Features.Accounts.Commands.Token;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class RefreshAccountTokenEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<RefreshTokenCommand>.WithActionResult<ApiResponse<RefreshTokenResponse>>
{
    private readonly ISender sender = sender;

    [HttpPost(Router.AccountRoute.RefreshToken)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "refresh token")]
    public override async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> HandleAsync(
        RefreshTokenCommand request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
