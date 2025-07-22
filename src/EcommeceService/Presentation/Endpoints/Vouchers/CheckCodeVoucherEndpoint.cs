using Application.Common.Auth;
using Application.Feature.Vouchers.Queries.CheckCode;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Vouchers
{
    public class CheckCodeVoucherEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CheckCodeQuery>.WithActionResult<
            ApiResponse<CheckCodeResponse>
        >
    {
        [HttpGet(Router.VoucherRoute.CheckCode)]
        [SwaggerOperation(Tags = [Router.VoucherRoute.Tags], Summary = "Create voucher")]
        [AuthorizeBy]
        public override async Task<ActionResult<ApiResponse<CheckCodeResponse>>> HandleAsync(
            CheckCodeQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var user = await sender.Send(request, cancellationToken);
            return user.ToCreatedResult();
        }
    }
}
