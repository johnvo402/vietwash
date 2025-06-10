using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Infrastructure.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Mediator;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper
    )
        : IRequestHandler<UpdateOrderCommand, UpdateOrderResponse>
    {

        public async ValueTask<UpdateOrderResponse> Handle(UpdateOrderCommand command, CancellationToken cancellationToken)
        {
            // Tìm Order theo OrderId
            Order order =
                await unitOfWork
                    .Repository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(long.Parse(command.OrderId)),
                        cancellationToken
                    )
                ?? throw new NotFoundException(
                    [Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()]
                );


            mapper.Map(command.Order, order);

            // Cập nhật Amount từ OrderItems nếu có
            if (command.Order.OrderItems is not null && command.Order.OrderItems.Any())
            {
                order.Amount = command.Order.OrderItems.Sum(i => i.Price * i.Quantity);

                foreach (var orderItemModel in command.Order.OrderItems)
                {
                    var existingItem = order.OrderItems.FirstOrDefault(x =>
                        x.Id != null && x.Id == orderItemModel.OrderItemId);

                    if (existingItem != null)
                    {
                        // Cập nhật item hiện có
                        existingItem.ServiceId = orderItemModel.ServiceId;
                        existingItem.UnitRelationId = orderItemModel.UnitRelationId;
                        existingItem.Price = orderItemModel.Price;
                    }
                    else
                    {
                        // Thêm mới OrderItem
                        var newItem = new OrderItem
                        {
                            ServiceId = orderItemModel.ServiceId,
                            UnitRelationId = orderItemModel.UnitRelationId,
                            Price = orderItemModel.Price
                        };
                        order.OrderItems.Add(newItem);
                    }
                }
            }

            decimal discountValue = command.Order.DiscountValue ?? 0m;
            order.Total = command.Order.DiscountType == null
                ? order.Amount // Không giảm giá nếu DiscountType null
                : command.Order.DiscountType.Value
                    ? order.Amount * (1 - discountValue / 100) // Giảm theo phần trăm
                    : order.Amount - discountValue;

            // Kiểm tra và cập nhật trạng thái
            if (command.Status.HasValue)
            {
                if (command.Status.Value < order.Status)
                    throw new BadRequestException(
                        [Messager.Create<Order>().Property(x => x.Status).Message(MessageType.Valid).Negative().Build()]);

                order.UpdateStatus(command.Status.Value);

                // Kiểm tra nếu Status chuyển sang Completed
                if (command.Status.Value == OrderStatus.Completed)
                {
                    // Tính tổng Payment hiện có
                    decimal totalPayments = order.OrderPayments.Sum(p => p.Amount);
                    decimal newPaymentAmount = command.Order.PaymentAmount ?? 0m;
                    decimal totalPaid = totalPayments + newPaymentAmount;

                    if (totalPaid < order.Total)
                        throw new BadRequestException(
                        [Messager
                        .Create<Order>()
                        .Property(x => x.Status)
                        .Message(MessageType.Valid)
                        .Negative()
                        .Build()]
                    );
                }
            }

            // Kiểm tra PaymentAmount cơ bản (nếu có)
            if (command.Order.PaymentAmount > 0 && command.Order.PaymentAmount < order.Total &&
                order.Status != OrderStatus.Completed)
            {
                throw new BadRequestException(
                    [Messager.Create<Order>().Property(x => x.Status).Message(MessageType.Valid).Negative().Build()]);
            }

            // Bắt đầu giao dịch
            try
            {
                DbTransaction transaction = await unitOfWork.CreateTransactionAsync(cancellationToken);

                // Cập nhật OrderPayment nếu có PaymentAmount
                if (command.Order.PaymentAmount.HasValue && command.Order.PaymentAmount > 0)
                {
                    order.OrderPayments.Add(new OrderPayment
                    {
                        OrderId = order.Id,
                        PaymentMethod = command.Order.PaymentMethod.Value,
                        Amount = command.Order.PaymentAmount.Value,
                        PaymentDate = DateTimeOffset.UtcNow
                    });
                }
                // Cập nhật Order
                await unitOfWork.Repository<Order>().UpdateAsync(order);
                await unitOfWork.SaveAsync(cancellationToken);

                // Commit giao dịch
                await unitOfWork.CommitAsync(cancellationToken);

                // Trả về response
                return mapper.Map<UpdateOrderResponse>(order);
            }
            catch (Exception)
            {
                // Rollback nếu có lỗi
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
