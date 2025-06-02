using Application.Features.Accounts.Commands.CustomerLogin;
using Application.Features.Accounts.Commands.Login;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class CustomerLoginEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<CustomerLoginCommand>.WithActionResult<
        ApiResponse<CustomerLoginResponse>
    >
{
    [HttpPost(Router.AccountRoute.CustomerLogin)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Logging in Account")]
    public override async Task<ActionResult<ApiResponse<CustomerLoginResponse>>> HandleAsync(
        CustomerLoginCommand request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
