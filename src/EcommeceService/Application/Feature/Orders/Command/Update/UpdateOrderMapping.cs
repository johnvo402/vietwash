using Application.Feature.Common.Projections.Orders;
using Domain.Aggregates.Orders;

namespace Application.Feature.Orders.Command.Update
{
    public static class UpdateOrderMapping
    {
        public static void FromUpdateModel(this Order entity, UpdateOrderModel model)
        {
            decimal amount = model.OrderItems.Sum(i => i.Price * i.Quantity);

            entity.Update(
                amount: amount,
                total: CalculationTotal(
                    amount,
                    entity.DiscountFixed,
                    entity.DiscountValue,
                    model.Point
                ),
                deliveryTime: model.DeliveryTime,
                tariffId: model.TariffId,
                note: model.Note,
                point: model.Point
            );
            entity.OrderItems.Clear();

            if (model.OrderItems != null)
            {
                foreach (var x in model.OrderItems)
                {
                    entity.OrderItems.Add(
                        new OrderItem
                        {
                            ServiceId = x.ServiceId,
                            UnitRelationId = x.UnitRelationId,
                            Price = x.Price,
                            Quantity = x.Quantity,
                            UnitRelationName = x.UnitRelationName,
                            ProcessingTime = x.ProcessingTime,
                            ServiceName = x.ServiceName,
                            UnitPrice = x.UnitPrice,
                        }
                    );
                }
            }
        }

        private static decimal CalculationTotal(
            decimal amount,
            bool discountFixed,
            decimal discountValue,
            decimal? point = null
        )
        {
            if (point.HasValue && point > 0)
            {
                amount -= point.Value * 10;
            }
            if (!discountFixed)
            {
                return amount - (amount * discountValue / 100);
            }
            else
            {
                return amount - discountValue;
            }
        }
    }
}
