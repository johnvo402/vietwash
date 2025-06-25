using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Command.UpdateStatus
{
    public class UpdateStatusCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string OrderId { get; set; } = string.Empty;
        public OrderStatus? Status { get; set; }
    }
}
