using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Funds.Queries.Detail
{
    public class GetFundDetailQuery : IRequest<GetFundDetailResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string FundId { get; set; } = default!;
    }
}
