using System;
using System.Collections.Generic;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Statistics.Queries.TopService
{
    public class GetTopServiceQuery : IRequest<IEnumerable<GetTopServiceResponse>>
    {


        [FromQuery]
        public string From { get; set; }

        [FromQuery]
        public string To { get; set; }

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
