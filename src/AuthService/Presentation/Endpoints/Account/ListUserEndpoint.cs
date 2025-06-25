using Application.Common.Auth;
using Application.Features.Accounts.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class ListAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListAccountQuery>.WithActionResult<
        ApiResponse<PaginationResponse<ListAccountResponse>>
    >
{
    [HttpGet(Router.AccountRoute.Accounts)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "list Account")]
    [AuthorizeBy]
    public override async Task<
        ActionResult<ApiResponse<PaginationResponse<ListAccountResponse>>>
    > HandleAsync(
        [FromQuery] ListAccountQuery request,
        CancellationToken cancellationToken = default
    ) => (await sender.Send(request, cancellationToken)).ToActionResult();
}
