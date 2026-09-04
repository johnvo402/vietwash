using Application.Common.Auth;
using Application.Feature.Vouchers.Queries.Detail;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.RouteResults;
using Application.Feature.Vouchers.Queries.VoucherUsageDetail;
namespace Presentation.Endpoints.Vouchers
{
    public class GetVoucherDetailEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<GetVoucherDetailQuery>.WithActionResult<
            ApiResponse<GetVoucherDetailResponse>
        >
    {
        [HttpGet(Routes.Router.VoucherRoute.GetDetail)]
        [SwaggerOperation(Tags = [Routes.Router.VoucherRoute.Tags], Summary = "Detail voucher")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<ActionResult<ApiResponse<GetVoucherDetailResponse>>> HandleAsync(
            GetVoucherDetailQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
