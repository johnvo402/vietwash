using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Funds.Queries.Detail
{
    public class GetFundDetailQuery : IRequest<Result<GetFundDetailResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long FundId { get; set; } = default!;
    }
}
