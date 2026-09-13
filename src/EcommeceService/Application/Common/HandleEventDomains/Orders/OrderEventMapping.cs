using Domain.Aggregates.Orders.Events;

namespace Application.Common.HandleEventDomains.Orders
{
    public static class OrderEventMapping
    {
        public static EInvoiceOrderMessage ToEInvoiceMessage(this EInvoiceEvent domainEvent)
        {
            return new EInvoiceOrderMessage
            {
                OrderId = domainEvent.OrderId,
                OrderCode = domainEvent.OrderCode,
                CompletedAt = domainEvent.CompletedAt,
                CustomerName = domainEvent.CustomerName,
                CustomerPhone = domainEvent.CustomerPhone,
                CustomerEmail = domainEvent.CustomerEmail,
                Vat = domainEvent.Vat,
                VatAmount = domainEvent.VatAmount,
                Total = domainEvent.Total,
                Discount = domainEvent.Discount,
                Items = domainEvent.Items
                    .Select(item => new EInvoiceOrderItemMessage
                    {
                        ServiceName = item.ServiceName,
                        UnitRelationName = item.UnitRelationName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                    })
                    .ToList(),
            };
        }
    }
}
