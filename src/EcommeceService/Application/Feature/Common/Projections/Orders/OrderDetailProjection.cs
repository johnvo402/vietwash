using Domain.Aggregates.Orders.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
	public class OrderDetailProjection : OrderProjection
	{
		public List<OrderItemProjection> OrderItems { get; set; } = [];
		public List<OrderPaymentProjection> OrderPayments { get; set; } = [];
	}
	public class OrderItemProjection
	{
		public string Id { get; set; }
		public string ServiceId { get; set; }
		public string UnitRelationId { get; set; }
		public decimal Price { get; set; }
	}

	public class OrderPaymentProjection
	{
		public string OrderId { get; set; }
		public PaymentMethod PaymentMethod { get; set; } // Có thể là string nếu đã đổi từ enum
		public decimal Amount { get; set; }
		public DateTimeOffset PaymentDate { get; set; }
	}
}
