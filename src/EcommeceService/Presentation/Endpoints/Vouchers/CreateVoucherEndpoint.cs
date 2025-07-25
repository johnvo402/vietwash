using Contracts.RouteResults;
using Application.Feature.Vouchers.Commands.Create;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;

namespace Presentation.Endpoints.Vouchers
{
    public class CreateVoucherEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<CreateVoucherCommand>.WithActionResult<ApiResponse>
    {
        [HttpPost(Router.VoucherRoute.Vouchers)]
        [SwaggerOperation(Tags = [Router.VoucherRoute.Tags], Summary = "Create voucher")]
        //[AuthorizeBy]
        public override async Task<ActionResult<ApiResponse>> HandleAsync(
            [FromBody] CreateVoucherCommand request,
            CancellationToken cancellationToken = default
        )
        {
            var user = await sender.Send(request, cancellationToken);
            return user.ToCreatedResult();
        }
    }
}
