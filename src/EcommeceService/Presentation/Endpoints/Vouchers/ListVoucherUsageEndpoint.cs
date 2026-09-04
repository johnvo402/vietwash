using Application.Common.Auth;
using Application.Feature.Vouchers.Queries.List;
using Ardalis.ApiEndpoints;
using Contracts.ApiWrapper;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Presentation.Routes;
using Swashbuckle.AspNetCore.Annotations;
using Contracts.RouteResults;
using Application.Feature.Vouchers.Queries.VoucherUsage;

namespace Presentation.Endpoints.Vouchers
{
    public class ListVoucherUsageEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListVoucherUsageQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListVoucherUsageResponse>>
        >
    {
        [HttpGet(Router.VoucherRoute.VoucherUsage)]
        [SwaggerOperation(Tags = [Router.VoucherRoute.Tags], Summary = "Voucher usage list")]
        [AuthorizeBy(roles: "ADMIN, MANAGER, STAFF")]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListVoucherUsageResponse>>>
        > HandleAsync(
            [FromQuery] ListVoucherUsageQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
