using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Queries.List
{
    public class ListOrderQuery : QueryParamRequest, IRequest<PaginationResponse<ListOrderResponse>>
    {
        [FromQuery]
        public string? From { get; set; }

        [FromQuery]
        public string? To { get; set; }

        [FromQuery]
        public string? BranchId { get; set; }
    }
}
