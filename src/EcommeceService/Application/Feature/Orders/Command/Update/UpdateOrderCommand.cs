using Application.Feature.Common.Projections.Orders;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long OrderId { get; set; }

        [FromBody]
        public UpdateOrderModel Model { get; set; }
    }
}
