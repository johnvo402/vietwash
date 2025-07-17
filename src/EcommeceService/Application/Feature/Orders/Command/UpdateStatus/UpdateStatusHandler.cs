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

                if (request.Status.HasValue)
                {
                    if (request.Status.Value < order.Status)
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
                    if (request.Status.Value == OrderStatus.Completed)
                    {
                        order.OrderPayments.Add(
                            new OrderPayment
                            {
                                Amount = order.Total,
                                PaymentMethod = (PaymentMethod)request.PaymentMethod!,
                                PaymentDate = DateTimeOffset.UtcNow,
                            }
                        );
                    }
                    order.EmitVoucherUsageEvent();
                    order.UpdateStatus(request.Status.Value);
                }
                using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

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
