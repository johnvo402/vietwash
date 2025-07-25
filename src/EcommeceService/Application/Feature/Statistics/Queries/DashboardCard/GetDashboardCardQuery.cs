using Application.Feature.Statistics.Queries.SaleResult;
using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.RevenueStatistic
{
    public class GetDashboardCardQuery : IRequest<Result<GetDashboardCardResponse>>
    {
        [FromQuery]
        public long BranchId { get; set; }
    }
}
