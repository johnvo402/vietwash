using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Vouchers.Queries.Detail
{
    public class GetVoucherDetailQuery : IRequest<Result<GetVoucherDetailResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long VoucherId { get; set; }
    }
}