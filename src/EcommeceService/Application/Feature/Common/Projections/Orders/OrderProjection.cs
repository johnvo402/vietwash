using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Common.Projections.Orders
{
	public class OrderProjection : BaseResponse
	{
		public string Code { get; set; } = string.Empty;
		public decimal Amount { get; set; }
		public decimal Total { get; set; }
		public bool DiscountType { get; set; }
		public decimal DiscountValue { get; set; }
		public Ulid? CustomerId { get; set; }
		public string Note { get; set; } = string.Empty;
		public DateTimeOffset OrderDate { get; set; }
		public OrderStatus Status { get; set; }
	}
}
