using Application.Feature.Orders.Common;
using Contracts.Utils;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.Create
{
    public static class CreateOrderMapping
    {
        internal static Order ToEntity(
            this CreateOrderCommand command,
            long staffId,
            int vat,
            ResolvedOrderPricing pricing,
            OrderPriceSummary totals,
            VoucherRedemption? voucher
        )
        {
            var order = new Order(
                branchId: command.BranchId,
                staffId: staffId,
                voucherId: voucher?.VoucherId,
                voucherCode: voucher?.Code,
                vat: vat,
                vatAmount: totals.VatAmount,
                code: Generator.GenerateCode("OD", 6),
                amount: totals.Amount,
                total: totals.Total,
                status: OrderStatus.Pending,
                customerId: command.CustomerId,
                discountFixed: voucher?.DiscountFixed ?? false,
                discountValue: voucher?.DiscountValue ?? 0,
                note: command.Note,
                deliveryTime: command.DeliveryTime,
                point: 0,
                tariffId: command.TariffId
            );

            order.OrderItems = pricing.Items.Select(ToOrderItem).ToList();
            return order;
        }

        private static OrderItem ToOrderItem(ResolvedOrderItem item) =>
            new()
            {
                ServiceId = item.ServiceId,
                UnitRelationId = item.UnitRelationId,
                Price = item.Price,
                Quantity = item.Quantity,
                UnitRelationName = item.UnitRelationName,
                ProcessingTime = item.ProcessingTime,
                ServiceName = item.ServiceName,
                UnitPrice = item.UnitPrice,
            };

        public static CreateOrderResponse ToCreateOrderResponse(this Order order)
        {
            var response = new CreateOrderResponse();
            response.MappingFrom(order);
            return response;
        }
    }
}
