using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.Orders.Specifications
{
	public class GetOrdersForServiceRevenueReportSpecification : Specification<Order>
	{
		public GetOrdersForServiceRevenueReportSpecification(DateTimeOffset? startDate, DateTimeOffset? endDate)
		{
			Query.Where(order =>
				(!startDate.HasValue || order.OrderDate >= startDate.Value) &&
				(!endDate.HasValue || order.OrderDate <= endDate.Value) &&
				order.Status == OrderStatus.Completed)
				.Include(x => x.OrderItems)
				.ThenInclude(oi => oi.Service)
				.Include(x => x.OrderItems) 
				.ThenInclude(oi => oi.UnitRelation)
				.Include(x => x.OrderPayments)
				.AsNoTracking()
				.AsSplitQuery();
		}
	}
}
