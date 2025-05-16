using Application.Common.Auth;
using Application.Features.Accounts.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System.Threading;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;

namespace Presentation.Endpoints.Account;

public class ListAccountEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListAccountQuery>.WithActionResult<ApiResponse<PaginationResponse<ListAccountResponse>>>
{
    [HttpGet(Router.AccountRoute.Accounts)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "list Account")]
    [AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.user}")]
    public override async Task<ActionResult<ApiResponse<PaginationResponse<ListAccountResponse>>>> HandleAsync(
        [FromQuery] ListAccountQuery request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
