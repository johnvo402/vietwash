using Domain.Aggregates.Orders;

namespace Application.Common.HandleEventDomains.Orders
{
    public static class OrderEventMapping
    {
        public static EInvoiceOrderMessage ToEInvoiceMessage(this Order order)
        {
            var disCount =
                (
                    order.DiscountFixed
                        ? order.DiscountValue
                        : order.DiscountValue * order.Total / 100
                )
                + order.Point * 10;
            return new EInvoiceOrderMessage
            {
                OrderId = order.Id,
                OrderCode = order.Code,
                CompletedAt = (DateTimeOffset)order.OrderDate!,

                CustomerName = order.Customer?.DisplayName ?? "",
                CustomerPhone = order.Customer?.PhoneNumber,
                CustomerEmail = order.Customer?.Email,
                Vat = order.Vat,
                VatAmount = order.VatAmount,

                Total = order.Amount,
                Discount = disCount,
                Items = order
                    .OrderItems.Select(i => new EInvoiceOrderItemMessage
                    {
                        ServiceName = i.ServiceName ?? i.Service.Name,
                        UnitRelationName = i.UnitRelationName,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                    })
                    .ToList(),
            };
        }
    }
}
