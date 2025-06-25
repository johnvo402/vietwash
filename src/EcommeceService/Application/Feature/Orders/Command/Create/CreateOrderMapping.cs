using Contracts.Extensions;
using Contracts.Utils;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.Create
{
    public static class CreateOrderMapping
    {
        public static Order ToEntity(this CreateOrderCommand command)
        {
            string code = Generator.GenerateCode("OD", 6);
            decimal amount = command.OrderItems.Sum(i => i.Price * i.Quantity);
            var response = new Order(
                customerId: command.CustomerId,
                branchId: command.BranchId,
                staffId: command.StaffId,
                code: code,
                amount: amount,
                total: CalculationTotal(amount, command.DiscountFixed, command.DiscountValue),
                discountFixed: command.DiscountFixed,
                discountValue: command.DiscountValue,
                note: command.Note,
                status: OrderStatus.Pending,
                orderDate: DateTimeOffset.UtcNow,
                deliveryTime: command.DeliveryTime
            );
            response.OrderItems = command.OrderItems.ToListMapping(x => new OrderItem
            {
                OrderId = x.OrderId,
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
            decimal discountValue
        )
        {
            if (discountFixed)
            {
                return amount - discountValue;
            }
            else
            {
                return amount - (amount * discountValue / 100);
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
