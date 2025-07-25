using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Vouchers.Queries.VoucherUsageDetail
{
    public class GetVoucherUsageDetailQuery : IRequest<Result<GetVoucherUsageDetailResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long VoucherUsageId { get; set; }
    }
}