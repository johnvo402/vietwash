using System.Linq.Expressions;
using Contracts.Extensions;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.GetLinkPayment
{
    public static class GetLinkPaymentMapping
    {
        public static Expression<Func<Order, OrderPayment>> Selector() =>
            order => new OrderPayment
            {
                Code = order.Code,
                Amount = (int)order.Total,
                Items = order
                    .OrderItems.AsEnumerable()
                    .Select(x => new OrderPaymentItem
                    {
                        Name = x.ServiceName,
                        Amount = (int)(x.Quantity * x.Price),
                        Quantity = x.Quantity,
                    })
                    .ToList(),
            };
    }

    public class OrderPayment
    {
        public string Code { get; set; } = string.Empty;
        public int Amount { get; set; }

        public ICollection<OrderPaymentItem> Items { get; set; } = [];
    }

    public class OrderPaymentItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Amount { get; set; }
    }
}
