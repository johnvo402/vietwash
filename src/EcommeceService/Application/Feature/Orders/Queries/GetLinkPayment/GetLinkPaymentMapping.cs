using System.Linq.Expressions;
using Contracts.Extensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Queries.GetLinkPayment
{
    public static class GetLinkPaymentMapping
    {
        public static Expression<Func<Order, OrderPayment>> Selector() =>
            order => new OrderPayment
            {
                Id = order.Id,
                BranchId = order.BranchId,
                Code = order.Code,
                Amount = order.Total,
                Status = order.Status,
                Items = order
                    .OrderItems.AsEnumerable()
                    .Select(x => new OrderPaymentItem
                    {
                        Name = x.ServiceName ?? string.Empty,
                        Amount = x.Price,
                        Quantity = x.Quantity,
                    })
                    .ToList(),
            };
    }

    public class OrderPayment
    {
        public long Id { get; set; }
        public long BranchId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; }

        public ICollection<OrderPaymentItem> Items { get; set; } = [];
    }

    public class OrderPaymentItem
    {
        public string Name { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
