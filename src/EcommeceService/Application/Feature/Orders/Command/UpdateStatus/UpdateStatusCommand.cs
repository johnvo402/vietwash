using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Command.Update;
using Contracts.Routers;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.UpdateStatus
{
    public class UpdateStatusCommand : IRequest<UpdateStatusResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string OrderId { get; set; } = string.Empty;
        public OrderStatus? Status { get; set; }
    }
}
