using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;


namespace Application.Feature.Orders.Queries.Detail
{
    public record GetOrderDetailQuery : IRequest<GetOrderDetailResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string OrderId { get; set; } = string.Empty;
    }
}
