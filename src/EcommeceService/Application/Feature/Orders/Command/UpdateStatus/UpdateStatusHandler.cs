using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Command.UpdateStatus
{
    public class UpdateStatusHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateStatusCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateStatusCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                Order? order = await unitOfWork
                    .DynamicReadOnlyRepository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(long.Parse(request.OrderId)),
                        cancellationToken
                    );
                if (order == null)
                {
                    return Result.Failure(
                        new NotFoundError(
                            "Order not found",
                            Messager
                                .Create<Order>()
                                .Message(MessageType.Found)
                                .Negative()
                                .BuildMessage()
                        )
                    );
                }

                if (request.Model.Status.HasValue)
                {
                    if (request.Model.Status.Value < order.Status)
                        return Result.Failure(
                            new BadRequestError(
                                "Status invalid",
                                Messager
                                    .Create<Order>()
                                    .Property(x => x.Status)
                                    .Message(MessageType.Valid)
                                    .Negative()
                                    .BuildMessage()
                            )
                        );
                    var equipmentOrder = new List<OrderEquipment>();
                    if (
                        request.Model.Status == OrderStatus.InProgress
                        && (
                            request.Model.OrderEquipments == null
                            || request.Model.OrderEquipments.Count == 0
                        )
                    )
                    {
                        return Result.Failure(
                            new BadRequestError(
                                "OrderEquipments Empty",
                                Messager
                                    .Create<Order>()
                                    .Property(x => x.OrderEquipments)
                                    .Message(MessageType.Empty)
                                    .Negative()
                                    .BuildMessage()
                            )
                        );
                    }
                    else
                    {
                        foreach (var x in request.Model.OrderEquipments!)
                        {
                            equipmentOrder.Add(
                                new OrderEquipment
                                {
                                    EquipmentId = x.EquipmentId,
                                    EquipmentName = x.EquipmentName,
                                }
                            );
                        }
                    }

                    order.UpdateStatus(request.Model.Status.Value, equipmentOrder);

                    if (
                        request.Model.Status == OrderStatus.Completed
                        && order.Status != OrderStatus.Completed
                    )
                    {
                        order.EmitVoucherUsageEvent(order.DiscountValue, order.VoucherId.Value);
                    }

                    order.PaymentMethod = request.Model.PaymentMethod;
                }
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Order>().UpdateAsync(order);
                await unitOfWork.SaveAsync(cancellationToken);

                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
