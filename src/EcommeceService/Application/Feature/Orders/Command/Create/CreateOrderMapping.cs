using Contracts.Extensions;
using Contracts.Utils;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.Create
{
    public static class CreateOrderMapping
    {
        public static Order ToEntity(this CreateOrderCommand command, long staffId, int vat)
        {
            string code = Generator.GenerateCode("OD", 6);
            decimal amount = command.OrderItems.Sum(i => i.Price * i.Quantity);
            decimal temptTotal = CalculationTotal(
                amount,
                command.DiscountFixed,
                command.DiscountValue,
                command.Point
            );
            decimal vatAmount = CalculateVatAmount(temptTotal, vat);
            var response = new Order(
                branchId: command.BranchId,
                staffId: staffId,
                voucherId: null,
                voucherCode: command.VoucherCode,
                vat: vat,
                vatAmount: vatAmount,
                code: code,
                amount: amount,
                total: temptTotal + vatAmount,
                status: OrderStatus.Pending,
                customerId: command.CustomerId,
                discountFixed: command.DiscountFixed,
                discountValue: command.DiscountValue,
                note: command.Note,
                deliveryTime: command.DeliveryTime,
                point: command.Point,
                tariffId: command.TariffId
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

        private static decimal CalculateVatAmount(decimal amount, int vatPercent)
        {
            if (vatPercent <= 0)
                return 0;
            return amount * vatPercent / 100;
        }

        public static CreateOrderResponse ToCreateOrderResponse(this Order order)
        {
            var response = new CreateOrderResponse();
            response.MappingFrom(order);
            return response;
        }
    }
}
