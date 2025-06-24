using Application.Common.Auth;
using Application.Features.Accounts.Queries.Detail;
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

public class GetAccountDetailEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse<GetAccountDetailResponse>>
{
    [HttpGet(Router.AccountRoute.GetUpdateDelete, Name = Router.AccountRoute.GetRouteName)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Detail Account")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse<GetAccountDetailResponse>>> HandleAsync(
        [FromRoute(Name = RouterBase.Id)] long userId,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(new GetAccountDetailQuery(userId), cancellationToken);
        return result.ToActionResult();
    }
}
