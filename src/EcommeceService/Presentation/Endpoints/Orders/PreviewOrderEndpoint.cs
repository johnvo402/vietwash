using Application.Common.Auth;
using Application.Feature.Orders.Queries.Preview;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Orders;

public sealed class PreviewOrderEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<PreviewOrderQuery>.WithActionResult<
        ApiResponse<PreviewOrderResponse>
    >
{
    [HttpPost(Router.OrderRoute.Orders + "/preview")]
    [SwaggerOperation(
        Tags = [Router.OrderRoute.Tags],
        Summary = "Preview order pricing without side effects"
    )]
    [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
    public override async Task<ActionResult<ApiResponse<PreviewOrderResponse>>> HandleAsync(
        [FromBody] PreviewOrderQuery request,
        CancellationToken cancellationToken = default
    ) => (await sender.Send(request, cancellationToken)).ToActionResult();
}
