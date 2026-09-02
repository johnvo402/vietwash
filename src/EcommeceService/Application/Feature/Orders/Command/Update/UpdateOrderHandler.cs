using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

namespace Application.Feature.Orders.Command.Update
{
    public class UpdateOrderHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
        : IRequestHandler<UpdateOrderCommand, Result>
    {
        public async ValueTask<Result> Handle(
            UpdateOrderCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!OrderActorAccess.IsStaffSide(currentAccount.Session?.Role))
                return Result.Failure(new ForbiddenError(Message.FORBIDDEN));

            try
            {
                _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
                Order? order = await unitOfWork
                    .DynamicReadOnlyRepository<Order>()
                    .FindByConditionAsync(
                        new GetOrderByIdSpecification(request.OrderId),
                        cancellationToken
                    );

                if (order is null)
                {
                    return await RollbackFailure(
                        new NotFoundError(
                            "Order not found.",
                            Messager.Create<Order>().Message(MessageType.Found).Negative().Build()
                        ),
                        cancellationToken
                    );
                }

                if (
                    !OrderActorAccess.CanOperateOrder(
                        currentAccount.Session?.Role,
                        currentAccount.Session?.Branches,
                        order.BranchId
                    )
                )
                {
                    return await RollbackFailure(
                        new ForbiddenError(Message.FORBIDDEN),
                        cancellationToken
                    );
                }

                if (!OrderLifecycle.CanEditDetails(order.Status))
                {
                    return await RollbackFailure(
                        new BadRequestError(
                            "Only pending orders can be updated.",
                            Messager.Create<Order>().Message(MessageType.Valid).Negative().Build()
                        ),
                        cancellationToken
                    );
                }

                Result<ResolvedOrderPricing> pricing = await OrderPricingResolver.ResolveAsync(
                    unitOfWork,
                    order.BranchId,
                    request.Model.TariffId,
                    request.Model.OrderItems,
                    DateTimeOffset.UtcNow,
                    cancellationToken
                );
                if (pricing.IsFailure)
                {
                    return await RollbackFailure(pricing.Error!, cancellationToken);
                }

                Result<OrderPriceSummary> totals = OrderPriceCalculator.Calculate(
                    pricing.Value!.Items,
                    order.DiscountFixed,
                    order.DiscountValue,
                    order.Vat
                );
                if (totals.IsFailure)
                {
                    return await RollbackFailure(totals.Error!, cancellationToken);
                }

                order.FromUpdateModel(request.Model, pricing.Value!, totals.Value!);
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

        private async Task<Result> RollbackFailure(
            ErrorDetails error,
            CancellationToken cancellationToken
        )
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return Result.Failure(error);
        }
    }
}
