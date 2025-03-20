using Application.Feature.Common.Projections.Orders;
using Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Create
{
	public class CreateOrderCommand : OrderModel, IRequest<CreateOrderResponse>
	{
	}

	public class CreateOrderResponse
	{
		public Ulid Id { get; set; }
		public string Code { get; set; } = string.Empty;
		public decimal Total { get; set; }
		public string Status { get; set; } = string.Empty;
		public DateTimeOffset OrderDate { get; set; }
	}
}
