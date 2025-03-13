using Application.Common.Auth;
using Application.Features.Users.Commands.Create;
using Ardalis.ApiEndpoints;
using CaseConverter;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Infrastructure.Constants;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.User;

public class CreateUserEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<CreateUserCommand>.WithActionResult<ApiResponse<CreateUserResponse>>
{
    [HttpPost(Router.UserRoute.Users)]
    [SwaggerOperation(Tags = [Router.UserRoute.Tags], Summary = "create User")]
    [AuthorizeBy(permissions: $"{ActionPermission.create}:{ObjectPermission.user}")]
    public override async Task<ActionResult<ApiResponse<CreateUserResponse>>> HandleAsync(
        [FromForm] CreateUserCommand request,
        CancellationToken cancellationToken = default
    )
    {
        CreateUserResponse user = await sender.Send(request, cancellationToken);
        return this.Created201(Router.UserRoute.GetRouteName, user.Id, user);
    }
}
