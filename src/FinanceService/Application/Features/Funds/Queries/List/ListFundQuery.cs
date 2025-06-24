using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Funds.Queries.List
{
    public class ListFundQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListFundResponse>>>
    {
        [FromQuery]
        public string? From { get; set; }

        [FromQuery]
        public string? To { get; set; }
    }
}
