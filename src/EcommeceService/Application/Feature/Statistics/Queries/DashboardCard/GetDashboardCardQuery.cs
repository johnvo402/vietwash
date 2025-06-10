using Application.Feature.Statistics.Queries.SaleResult;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.RevenueStatistic
{
    public class GetDashboardCardQuery : IRequest<IEnumerable<GetDashboardCardResponse>>
    {
        [FromQuery]
        public string BranchId { get; set; }


    }

}
