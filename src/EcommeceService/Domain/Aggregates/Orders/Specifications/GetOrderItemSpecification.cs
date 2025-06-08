using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Linq;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderItemSpecification : Specification<Order>
    {
        public GetOrderItemSpecification(DateTime from, DateTime to, int branchId)
        {

            Query.Where(order =>
                order.OrderDate >= from &&
                order.OrderDate < to &&
                order.Status == OrderStatus.Completed &&
                order.BranchId == branchId
                )
            .Include(order => order.OrderItems)
            .AsNoTracking();
        }
    }
}
