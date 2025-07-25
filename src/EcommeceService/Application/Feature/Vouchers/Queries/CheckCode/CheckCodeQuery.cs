using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Vouchers.Queries.CheckCode
{
    public class CheckCodeQuery : IRequest<Result<CheckCodeResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public required string VoucherCode { get; set; }

        [FromRoute(Name = "CustomerId")]
        public required long CustomerId { get; set; }
    }
}
