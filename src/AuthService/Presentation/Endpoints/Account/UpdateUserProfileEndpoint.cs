using Application.Common.Auth;
using Application.Features.Accounts.Commands.Profiles;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class UpdateAccountProfileEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<UpdateAccountProfileCommand>.WithActionResult<ApiResponse<UpdateAccountProfileResponse>>
{
    [HttpPut(Router.AccountRoute.Profile)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Update Profile Account")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse<UpdateAccountProfileResponse>>> HandleAsync(
         UpdateAccountProfileCommand request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
