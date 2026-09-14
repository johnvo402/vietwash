using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.UpdateStatus;

public class UpdateStatusHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentAccount,
    TimeProvider? timeProvider = null
) : IRequestHandler<UpdateStatusCommand, Result>
{
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public async ValueTask<Result> Handle(
        UpdateStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!long.TryParse(request.OrderId, out long orderId) || orderId <= 0)
            return Failure("OrderId invalid");
        if (request.Model?.Status is not OrderStatus target)
            return Failure("Status invalid");
        ErrorDetails? actorError = OrderStatusRequestPolicy.ValidateActor(
            request,
            currentAccount
        );
        if (actorError is not null)
            return Result.Failure(actorError);

        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            Order? order = await unitOfWork
                .DynamicRepository<Order>()
                .FindByConditionAsync(new GetOrderByIdSpecification(orderId), cancellationToken);
            if (order is null)
                return await RollbackFailure(
                    new NotFoundError(
                        "Order not found",
                        Messager
                            .Create<Order>()
                            .Message(MessageType.Found)
                            .Negative()
                            .BuildMessage()
                    ),
                    cancellationToken
                );

            ErrorDetails? orderError = OrderStatusRequestPolicy.ValidateOrder(
                request,
                currentAccount,
                order,
                target
            );
            if (orderError is not null)
                return await RollbackFailure(
                    orderError,
                    cancellationToken
                );

            CancellationPreparation cancellationPreparation =
                OrderStatusRequestPolicy.PrepareCancellation(
                    request,
                    currentAccount,
                    target,
                    _timeProvider
                );
            if (cancellationPreparation.Error is not null)
                return await RollbackFailure(
                    cancellationPreparation.Error,
                    cancellationToken
                );
            OrderCancellation? cancellation = cancellationPreparation.Cancellation;

            long[] requestedEquipmentIds =
                request.Model.OrderEquipments?.Select(x => x.EquipmentId).ToArray() ?? [];
            OrderTransitionResult evaluation = order.EvaluateTransition(
                target,
                request.Model.PaymentMethod,
                requestedEquipmentIds.Length,
                cancellation
            );
            if (evaluation == OrderTransitionResult.Idempotent)
            {
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            if (evaluation != OrderTransitionResult.Applied)
                return await RollbackFailure(
                    OrderStatusRequestPolicy.BadRequest(
                        OrderStatusRequestPolicy.GetTransitionError(evaluation)
                    ),
                    cancellationToken
                );

            OrderStatus previousStatus = order.Status;
            OrderCancellationResourcePlan cancellationPlan = OrderCancellationResourcePolicy.Create(
                previousStatus,
                target,
                order.CustomerId,
                order.VoucherId
            );
            IReadOnlyList<OrderEquipment> resolvedEquipments = [];
            EquipmentLifecycleAction equipmentAction = EquipmentSelectionPolicy.GetLifecycleAction(
                previousStatus,
                target
            );

            if (cancellationPlan.RequiresPayOsCoordination)
            {
                ErrorDetails? schedulingError =
                    await OrderCancellationCoordinator.SchedulePayOsAsync(
                        unitOfWork,
                        order,
                        cancellation!,
                        cancellationToken
                    );
                if (schedulingError is not null)
                    return await RollbackFailure(
                        schedulingError,
                        cancellationToken
                    );

                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }

            EquipmentSelectionResult equipmentResult =
                await OrderEquipmentLifecycle.ResolveAsync(
                    unitOfWork,
                    order,
                    equipmentAction,
                    requestedEquipmentIds,
                    cancellationToken
                );
            if (!equipmentResult.IsSuccess)
                return await RollbackFailure(
                    OrderStatusRequestPolicy.BadRequest(
                        $"Equipment selection invalid: {equipmentResult.FailureReason}."
                    ),
                    cancellationToken
                );
            resolvedEquipments = equipmentResult.Equipments;

            OrderTransitionWriteResult writeResult = await OrderTransitionPersistence.ReserveAsync(
                unitOfWork,
                order,
                previousStatus,
                target,
                cancellationToken
            );
            if (writeResult != OrderTransitionWriteResult.Applied)
            {
                await unitOfWork.RollbackAsync(cancellationToken);
                return writeResult == OrderTransitionWriteResult.AlreadyApplied
                    ? Result.Success()
                    : ConcurrencyFailure();
            }

            ErrorDetails? cancellationError =
                await OrderCancellationCoordinator.ReleaseLocalResourcesAsync(
                    unitOfWork,
                    order,
                    cancellationPlan,
                    cancellationToken
                );
            if (cancellationError is not null)
                return await RollbackFailure(cancellationError, cancellationToken);

            bool equipmentsApplied = await OrderEquipmentLifecycle.ApplyAsync(
                unitOfWork,
                order,
                equipmentAction,
                requestedEquipmentIds,
                cancellationToken
            );
            if (!equipmentsApplied)
                return await RollbackFailure(
                    OrderStatusRequestPolicy.BadRequest(
                        "One or more equipments were claimed concurrently."
                    ),
                    cancellationToken
                );

            if (equipmentAction == EquipmentLifecycleAction.Claim)
            {
                OrderMaterialConsumptionResult materialResult =
                    await OrderMaterialConsumption.ConsumeAsync(
                        unitOfWork,
                        order,
                        cancellationToken
                    );
                if (!materialResult.IsSuccess)
                    return await RollbackFailure(
                        OrderStatusRequestPolicy.BadRequest(
                            materialResult.ErrorMessage ?? "Material consumption failed."
                        ),
                        cancellationToken
                    );
            }

            OrderTransitionResult applied = order.TransitionTo(
                target,
                request.Model.PaymentMethod,
                resolvedEquipments,
                cancellation: cancellation
            );
            if (applied != OrderTransitionResult.Applied)
                throw new InvalidOperationException(
                    $"Order transition changed after validation: {applied}."
                );

            order.AdvanceVersion();
            await unitOfWork.SaveAsync(cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            await unitOfWork.RollbackAsync(cancellationToken);
            return ConcurrencyFailure();
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

    private static Result Failure(string message) =>
        Result.Failure(OrderStatusRequestPolicy.BadRequest(message));

    private static Result ConcurrencyFailure() =>
        Result.Failure(
            new ConflictError(
                "Order was changed by another request. Please reload and try again.",
                Messager.Create<Order>().Message(MessageType.Valid).Negative().Build()
            )
        );

}
