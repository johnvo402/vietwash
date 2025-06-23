using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Funds.Queries.List
{
    public class ListFundQuery : QueryParamRequest, IRequest<PaginationResponse<ListFundResponse>>
    {
        [FromQuery]
        public string? From { get; set; }

        [FromQuery]
        public string? To { get; set; }
    }
}
