using System.Linq.Expressions;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.List
{
    public static class ListOrderMapping
    {
        public static Expression<Func<Order, ListOrderResponse>> Selector() =>
            order => new ListOrderResponse
            {
                //base mapping
                Id = order.Id,
                PublicId = order.PublicId,
                CreatedAt = order.CreatedAt,
                CreatedBy = order.CreatedBy,
                UpdatedAt = order.UpdatedAt,
                UpdatedBy = order.UpdatedBy,

                Code = order.Code,
                Amount = order.Amount,
                Total = order.Total,
                DiscountFixed = order.DiscountFixed,
                DiscountValue = order.DiscountValue,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                DeliveryTime = order.DeliveryTime,
                Status = order.Status,
                BranchId = order.BranchId,
            };
    }
}
