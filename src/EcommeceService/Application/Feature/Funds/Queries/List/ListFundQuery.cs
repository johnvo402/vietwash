using Application.Feature.Orders.Queries.List;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Funds.Queries.List
{
    public class ListFundQuery : QueryParamRequest, IRequest<PaginationResponse<ListFundResponse>>
    {

        [FromQuery]
        public string From { get; set; }

        [FromQuery]
        public string To { get; set; }
    }
}
