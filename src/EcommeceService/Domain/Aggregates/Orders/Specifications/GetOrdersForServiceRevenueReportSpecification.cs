using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Aggregates.Orders.Enums;
using Specification;
using Specification.Builders;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrdersForServiceRevenueReportSpecification : Specification<Order>
    {
        public GetOrdersForServiceRevenueReportSpecification(
            DateTimeOffset? startDate,
            DateTimeOffset? endDate
        )
        {
            Query
                .Where(order =>
                    (!startDate.HasValue || order.OrderDate >= startDate.Value)
                    && (!endDate.HasValue || order.OrderDate <= endDate.Value)
                    && order.Status == OrderStatus.Completed
                )
                .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.Service)
                .Include(x => x.OrderItems)
                .ThenInclude(oi => oi.UnitRelation)
                .AsNoTracking()
                .AsSplitQuery();
        }
    }
}
