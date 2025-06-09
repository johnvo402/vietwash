using Domain.Aggregates.Orders.Enums;
using JohnChum.SharedKernel.Domain.Common.Specs;
using System;
using System.Linq;

namespace Domain.Aggregates.Orders.Specifications
{
    public class GetOrderItemSpecification : Specification<Order>
    {
        public GetOrderItemSpecification(DateTime from, DateTime to, int branchId, List<string> branchs)
        {

            Query.Where(order =>
                order.OrderDate >= from &&
                order.OrderDate < to &&
                order.Status == OrderStatus.Completed &&
                order.BranchId == branchId &&
                  branchs.Contains(order.BranchId.ToString())
                )
            .Include(order => order.OrderItems)
            .AsNoTracking();
        }
    }
}
