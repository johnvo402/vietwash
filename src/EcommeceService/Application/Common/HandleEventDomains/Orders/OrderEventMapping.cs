using Domain.Aggregates.Orders;

namespace Application.Common.HandleEventDomains.Orders
{
    public static class OrderEventMapping
    {
        public static EInvoiceOrderMessage ToEInvoiceMessage(this Order order)
        {
            return new EInvoiceOrderMessage
            {
                OrderId = order.Id,
                OrderCode = order.Code,
                CompletedAt = (DateTimeOffset)order.OrderDate!,

                CustomerName = order.Customer?.DisplayName ?? "",
                CustomerPhone = order.Customer?.PhoneNumber,
                CustomerEmail = order.Customer?.Email,

                Total = order.Total,

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
