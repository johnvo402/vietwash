using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Queries.Detail
{
    public record GetOrderDetailQuery : IRequest<Result<GetOrderDetailResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long OrderId { get; set; }
    }
}
