using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.TopService
{
    public class GetTopServiceQuery : IRequest<Result<IEnumerable<GetTopServiceResponse>>>
    {
        [FromQuery]
        public string From { get; set; } = default!;

        [FromQuery]
        public string To { get; set; } = default!;

        [FromQuery]
        public string BranchId { get; set; } = default!;

        //public string From { get; }
        //public string To { get; }

        //public GetTopServiceQuery(DateTime from, DateTime to)
        //{
        //    if (DateTime.Parse(From) > DateTime.Parse(To))
        //    {
        //        throw new ArgumentException("to must be greater than from");
        //    }

        //    From = from.ToString();
        //    To = to.ToString();
        //}
    }
}
