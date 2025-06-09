using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.BranchNetRevenue
{
    public class GetNetRevenueBranchQuery : IRequest<IEnumerable<GetNetRevenueBranchResponse>>
    {
        [FromQuery]
        public string? From { get; set; }

        public string? To { get; set; }
    }
}
