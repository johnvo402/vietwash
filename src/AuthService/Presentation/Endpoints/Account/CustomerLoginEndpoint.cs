using Application.Features.Accounts.Commands.CustomerLogin;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class CustomerLoginEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<CustomerLoginCommand>.WithActionResult<ApiResponse>
{
    [HttpPost(Router.AccountRoute.CustomerLogin)]
    [AllowAnonymous]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Logging in Account")]
    public override async Task<ActionResult<ApiResponse>> HandleAsync(
        CustomerLoginCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var result = await sender.Send(request, cancellationToken);
        return result.ToActionResult();
    }
}
