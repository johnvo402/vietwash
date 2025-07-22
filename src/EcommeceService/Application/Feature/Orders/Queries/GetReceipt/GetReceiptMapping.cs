using Application.Feature.Common.Projections.Receipts;
using Contracts.Utils;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Queries.GetReceipt
{
    public static class GetReceiptMapping
    {
        public static ReceiptModel MapToReceiptModel(this Order order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var receiptModel = new ReceiptModel
            {
                Customer = new CustomerInfoReceipt
                {
                    DisplayName = order.Customer?.DisplayName ?? string.Empty,
                    PhoneNumber = order.Customer?.DisplayName ?? string.Empty,
                },
                OrderDate = order.OrderDate,
                OrderItems =
                    order
                        .OrderItems?.Select(item => new OrderItemReceipt
                        {
                            ServiceName = item.ServiceName ?? string.Empty,
                            UnitRelationName = item.UnitRelationName ?? string.Empty,
                            Quantity = item.Quantity,
                            UnitPrice = NumberToTextConverter.FormatCurrency(item.UnitPrice),
                            TotalPriceItem = NumberToTextConverter.FormatCurrency(
                                item.Quantity * item.UnitPrice
                            ),
                        })
                        .ToList() ?? new List<OrderItemReceipt>(),
                Total = NumberToTextConverter.FormatCurrency(order.Total),
                TotalInWords = NumberToTextConverter.ToVietnameseCurrencyText(order.Total),
            };

            return receiptModel;
        }
    }
}
