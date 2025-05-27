using Application.Common.Auth;
using Application.Features.Accounts.Commands.Logout;
using Ardalis.ApiEndpoints;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Contracts.RouteResults;
using Presentation.Routes;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account
{
    public class LogoutEndpoint(ISender sender)
    : EndpointBaseAsync.WithoutRequest.WithActionResult<ApiResponse<LogoutResponse>>
    {
        [HttpPost(Router.AccountRoute.Logout)]
        [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Logout in Account")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<LogoutResponse>>> HandleAsync(
            CancellationToken cancellationToken = default
        )
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var command = new LogoutCommand { Token = token };
           return this.Ok200(await sender.Send(command, cancellationToken));
        }
    }
}
