using Application.Common.Auth;
using Application.Features.Accounts.Commands.RequestOtp;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class RequestOtpEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<RequestOtpCommand>.WithActionResult<ApiResponse>
{
    [HttpPost(Router.AccountRoute.RequestOtp)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Logging in Account")]
    [AuthorizeBy]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        RequestOtpCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
