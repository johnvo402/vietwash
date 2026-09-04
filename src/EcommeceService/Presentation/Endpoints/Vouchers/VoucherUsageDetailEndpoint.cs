using Ardalis.ApiEndpoints;
using Application.Common.Auth;
using Contracts.ApiWrapper;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations; 
using Mediator; 
using Contracts.RouteResults;
using Application.Feature.Vouchers.Queries.VoucherUsageDetail;
namespace Presentation.Endpoints.Vouchers
{
    public class VoucherUsageDetailEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetVoucherUsageDetailQuery>.WithActionResult<
            ApiResponse<GetVoucherUsageDetailResponse>
        >
    {
        [HttpGet(Routes.Router.VoucherRoute.VoucherUsageDetail)]
        [SwaggerOperation(Tags = [Routes.Router.VoucherRoute.Tags], Summary = "Detail voucher")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse<GetVoucherUsageDetailResponse>>> HandleAsync(
            GetVoucherUsageDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
