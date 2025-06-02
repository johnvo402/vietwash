using Application.Common.Auth;
using Application.Features.Accounts.Queries.Detail;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.Routers;

namespace Presentation.Endpoints.Account;

public class GetAccountDetailEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse<GetAccountDetailResponse>>
{
    [HttpGet(Router.AccountRoute.GetUpdateDelete, Name = Router.AccountRoute.GetRouteName)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Detail Account")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse<GetAccountDetailResponse>>> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] long userId,
        CancellationToken cancellationToken = default
    ) =>
        this.Ok200(
            await sender.Send(new GetAccountDetailQuery(userId), cancellationToken)
        );
}
