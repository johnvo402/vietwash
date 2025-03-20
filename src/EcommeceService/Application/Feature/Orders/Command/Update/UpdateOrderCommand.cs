using Application.Feature.Common.Projections.Orders;
using Application.Feature.Common.Projections.Units;
using Contracts.Routers;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Update
{
	public class UpdateOrderCommand : IRequest<UpdateOrderResponse>
	{
		[FromRoute(Name = RouterBase.Id)]
		public string OrderId { get; set; } = string.Empty;
		public OrderStatus? Status { get; set; }
		[FromBody]
		public OrderModel Order { get; set; } = new OrderModel();
	}
}
