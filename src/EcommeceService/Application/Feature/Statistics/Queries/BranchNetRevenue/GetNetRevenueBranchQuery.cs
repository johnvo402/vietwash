using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.BranchNetRevenue
{
    public class GetNetRevenueBranchQuery
        : IRequest<Result<IEnumerable<GetNetRevenueBranchResponse>>>
    {
        [FromQuery]
        public string? From { get; set; }

        public string? To { get; set; }
    }
}
