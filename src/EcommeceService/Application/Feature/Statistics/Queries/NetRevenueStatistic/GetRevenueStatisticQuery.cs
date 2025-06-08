using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.RevenueStatistic
{
    public class GetRevenueStatisticQuery : IRequest<IEnumerable<GetRevenueStatisticResponse>>
    {
        [FromQuery]
        public string BranchId { get; set; }

        [FromQuery]
        public string From { get; set; }

        [FromQuery]
        public string To { get; set; }
    }
}
