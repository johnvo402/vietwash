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
		public Ulid Id { get; set; }
		public Ulid ServiceId { get; set; }
		public Ulid UnitRelationId { get; set; }
		public decimal Price { get; set; }
	}

	public enum OrderPaymentProjection : byte
	{
		Cash = 1,
		Card = 2
	}
}
