using Contracts.ApiWrapper;
using Domain.Functions;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.RevenueStatistic
{
    public class GetRevenueStatisticQuery : IRequest<Result<IEnumerable<GetRevenueStatistic>>>
    {
        [FromQuery]
        public string BranchId { get; set; } = default!;

        [FromQuery]
        public string From { get; set; } = default!;

        [FromQuery]
        public string To { get; set; } = default!;
    }
}
