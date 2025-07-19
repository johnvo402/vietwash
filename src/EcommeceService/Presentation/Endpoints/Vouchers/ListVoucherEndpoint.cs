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

namespace Presentation.Endpoints.Vouchers
{
    public class ListVoucherEndpoint(ISender sender)
        : EndpointBaseAsync.WithRequest<ListVoucherQuery>.WithActionResult<
            ApiResponse<PaginationResponse<ListVoucherResponse>>
        >
    {
        [HttpGet(Router.VoucherRoute.Vouchers)]
        [SwaggerOperation(Tags = [Router.VoucherRoute.Tags], Summary = "Voucher list")]
        //[AuthorizeBy]
        public override async Task<
            ActionResult<ApiResponse<PaginationResponse<ListVoucherResponse>>>
        > HandleAsync(
            [FromQuery] ListVoucherQuery request,
            CancellationToken cancellationToken = default
        )
        {
            var result = await sender.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
