using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Contracts.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Queries.List
{
    public class ListOrderQuery
        : QueryParamRequest,
            IRequest<Result<PaginationResponse<ListOrderResponse>>>
    {
        [FromQuery]
        public string? From { get; set; }

        [FromQuery]
        public string? To { get; set; }

        [FromQuery]
        public string? BranchId { get; set; }
    }
}
