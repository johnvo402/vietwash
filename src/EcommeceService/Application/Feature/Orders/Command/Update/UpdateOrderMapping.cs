using Application.Feature.Common.Projections.Orders;
using Application.Feature.Orders.Common;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Command.Update
{
    public static class UpdateOrderMapping
    {
        internal static void FromUpdateModel(
            this Order entity,
            UpdateOrderModel model,
            ResolvedOrderPricing pricing,
            OrderPriceSummary totals
        )
        {
            entity.Update(
                amount: totals.Amount,
                vatAmount: totals.VatAmount,
                total: totals.Total,
                deliveryTime: model.DeliveryTime,
                tariffId: model.TariffId,
                note: model.Note,
                point: 0
            );

            entity.OrderItems.Clear();
            foreach (ResolvedOrderItem item in pricing.Items)
            {
                entity.OrderItems.Add(
                    new OrderItem
                    {
                        ServiceId = item.ServiceId,
                        UnitRelationId = item.UnitRelationId,
                        Price = item.Price,
                        Quantity = item.Quantity,
                        UnitRelationName = item.UnitRelationName,
                        ProcessingTime = item.ProcessingTime,
                        ServiceName = item.ServiceName,
                        UnitPrice = item.UnitPrice,
                    }
                );
            }
        }
    }
}
