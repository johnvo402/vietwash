using Application.Common.Auth;
using Application.Features.Accounts.Queries.Profiles;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class GetAccountProfileEndpoint(ISender sender)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<ApiResponse<GetAccountProfileResponse>>
{
    [HttpGet(Router.AccountRoute.Profile)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Profile Account")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse<GetAccountProfileResponse>>> HandleAsync(
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(new GetAccountProfileQuery(), cancellationToken));
}
