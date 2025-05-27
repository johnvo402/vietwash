using Application.Common.Auth;
using Application.Features.Users.Queries.List;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using System.Threading;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;

namespace Presentation.Endpoints.User;

public class ListUserEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<ListUserQuery>.WithActionResult<ApiResponse<PaginationResponse<ListUserResponse>>>
{
    [HttpGet(Router.UserRoute.Users)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "list User")]
    [AuthorizeBy(permissions: $"{ActionPermission.list}:{ObjectPermission.user}")]
    public override async Task<ActionResult<ApiResponse<PaginationResponse<ListUserResponse>>>> HandleAsync(
        [FromQuery] ListUserQuery request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
