using Application.Common.Auth;
using Application.Features.Users.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public sealed class GetCustomerEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<long>.WithActionResult<ApiResponse<GetCustomerResponse>>
{
    [HttpGet(Router.UserRoute.Users + "/{id:long}")]
    [SwaggerOperation(
        Tags = [Router.UserRoute.Tags],
        Summary = "Find an active synchronized customer"
    )]
    [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
    public override async Task<ActionResult<ApiResponse<GetCustomerResponse>>> HandleAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken = default
    ) => (await sender.Send(new GetCustomerQuery(id), cancellationToken)).ToActionResult();
}
