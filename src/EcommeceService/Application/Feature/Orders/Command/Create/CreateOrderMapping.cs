using Contracts.Extensions;
using Contracts.Utils;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.Create
{
    public static class CreateOrderMapping
    {
        public static Order ToEntity(this CreateOrderCommand command, long staffId)
        {
            string code = Generator.GenerateCode("OD", 6);
            decimal amount = command.OrderItems.Sum(i => i.Price * i.Quantity);
            var response = new Order(
                branchId: command.BranchId,
                staffId: staffId,
                voucherId: null,
                voucherCode: command.VoucherCode,
                code: code,
                amount: amount,
                total: CalculationTotal(
                    amount,
                    command.DiscountFixed,
                    command.DiscountValue,
                    command.Point
                ),
                status: OrderStatus.Pending,
                customerId: command.CustomerId,
                discountFixed: command.DiscountFixed,
                discountValue: command.DiscountValue,
                note: command.Note,
                deliveryTime: command.DeliveryTime,
                point: command.Point
            );

            response.OrderItems = command.OrderItems.ToListMapping(x => new OrderItem
            {
                ServiceId = x.ServiceId,
                UnitRelationId = x.UnitRelationId,
                Price = x.Price,
                Quantity = x.Quantity,
                UnitRelationName = x.UnitRelationName,
                ProcessingTime = x.ProcessingTime,
                ServiceName = x.ServiceName,
                UnitPrice = x.UnitPrice,
            });

            return response;
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
                amount -= point.Value;
            }
            if (discountFixed)
            {
                return amount - (amount * discountValue / 100);
            }
            else
            {
                return amount - discountValue;
            }
        }

        public static CreateOrderResponse ToCreateOrderResponse(this Order order)
        {
            var response = new CreateOrderResponse();
            response.MappingFrom(order);
            return response;
        }
    }
}
