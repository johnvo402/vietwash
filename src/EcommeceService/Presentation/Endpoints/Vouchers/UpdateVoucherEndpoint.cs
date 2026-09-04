using Application.Common.Auth;
using Application.Feature.Vouchers.Commands.Update;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.RouteResults;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Vouchers
{
    public class UpdateVoucherEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<UpdateVoucherCommand>.WithActionResult<ApiResponse>
    {
        [HttpPut(Router.VoucherRoute.GetUpdateDelete)]
        [SwaggerOperation(Tags = [Router.VoucherRoute.Tags], Summary = "Update voucher")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            UpdateVoucherCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request);
            return result.ToActionResult();
        }
    }
}
