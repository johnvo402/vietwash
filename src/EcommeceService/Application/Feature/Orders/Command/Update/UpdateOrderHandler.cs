using Application.Common.Errors;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateOrderCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateOrderCommand request,
            CancellationToken cancellationToken
        )
        {
            Order? order = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .FindByConditionAsync(
                    new GetOrderByIdSpecification(request.OrderId),
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
            order.FromUpdateModel(request.Model);

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);

                await unitOfWork.Repository<Order>().UpdateAsync(order);

                await unitOfWork.SaveAsync(cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
