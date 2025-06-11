using Application.Features.Accounts.Commands.VerifyOtpLoginCustomer;
using Ardalis.ApiEndpoints;
using Contracts.RouteResults;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Account;

public class CustomerLoginVerifyOtpEndpoint(ISender sender)
    : EndpointBaseAsync.WithRequest<VerifyOtpCommand>.WithActionResult<
        ApiResponse<VerifyOtpResponse>
    >
{
    [HttpPost(Router.AccountRoute.CustomerLoginVerify)]
    [SwaggerOperation(Tags = [Router.AccountRoute.Tags], Summary = "Logging in Account Verify")]
    public override async Task<ActionResult<ApiResponse<VerifyOtpResponse>>> HandleAsync(
        VerifyOtpCommand request,
        CancellationToken cancellationToken = default
    ) => this.Ok200(await sender.Send(request, cancellationToken));
}
