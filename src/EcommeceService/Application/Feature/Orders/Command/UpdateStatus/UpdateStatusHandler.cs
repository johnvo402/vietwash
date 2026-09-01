using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Equipments;
using Domain.Aggregates.Equipments.Enums;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Command.UpdateStatus;

public class UpdateStatusHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<UpdateStatusCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        if (!long.TryParse(request.OrderId, out long orderId) || orderId <= 0)
            return Failure("OrderId invalid");
        if (request.Model?.Status is not OrderStatus target)
            return Failure("Status invalid");

        try
        {
            _ = await unitOfWork.BeginTransactionAsync(cancellationToken);
            Order? order = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .FindByConditionAsync(new GetOrderByIdSpecification(orderId), cancellationToken);
            if (order is null)
                return await RollbackFailure(
                    new NotFoundError(
                        "Order not found",
                        Messager.Create<Order>().Message(MessageType.Found).Negative().BuildMessage()
                    ),
                    cancellationToken
                );

            if (
                !request.IsVerifiedPayOsWebhook
                && !OrderBranchAccess
                    .FromSession(currentAccount.Session?.Branches)
                    .IsAuthorized(order.BranchId)
            )
                return await RollbackFailure(
                    new ForbiddenError(Message.FORBIDDEN),
                    cancellationToken
                );

            if (request.ExpectedPaymentAmount.HasValue)
            {
                if (
                    !PayOsOrderPolicy.TryGetAmount(order.Total, out int authoritativeAmount)
                    || authoritativeAmount != request.ExpectedPaymentAmount.Value
                )
                    return await RollbackFailure(
                        CreateBadRequest("Payment amount does not match the order total."),
                        cancellationToken
                    );
            }

            long[] requestedEquipmentIds =
                request.Model.OrderEquipments?.Select(x => x.EquipmentId).ToArray() ?? [];
            OrderTransitionResult evaluation = order.EvaluateTransition(
                target,
                request.Model.PaymentMethod,
                requestedEquipmentIds.Length
            );
            if (evaluation == OrderTransitionResult.Idempotent)
            {
                await unitOfWork.CommitAsync(cancellationToken);
                return Result.Success();
            }
            if (evaluation != OrderTransitionResult.Applied)
                return await RollbackFailure(
                    CreateBadRequest(GetTransitionError(evaluation)),
                    cancellationToken
                );

            OrderStatus previousStatus = order.Status;
            IReadOnlyList<OrderEquipment> resolvedEquipments = [];
            EquipmentLifecycleAction equipmentAction =
                EquipmentSelectionPolicy.GetLifecycleAction(previousStatus, target);
            if (equipmentAction == EquipmentLifecycleAction.Claim)
            {
                List<EquipmentSnapshot> candidates = await unitOfWork
                    .Repository<Equipment>()
                    .QueryAsync(x => requestedEquipmentIds.Contains(x.Id))
                    .Select(x => new EquipmentSnapshot(
                        x.Id,
                        x.Name,
                        x.BranchId,
                        x.Status,
                        x.Using
                    ))
                    .ToListAsync(cancellationToken);
                EquipmentSelectionResult equipmentResult = EquipmentSelectionPolicy.Resolve(
                    order.BranchId,
                    requestedEquipmentIds,
                    candidates
                );
                if (!equipmentResult.IsSuccess)
                    return await RollbackFailure(
                        CreateBadRequest(
                            $"Equipment selection invalid: {equipmentResult.FailureReason}."
                        ),
                        cancellationToken
                    );
                resolvedEquipments = equipmentResult.Equipments;
            }

            int transitionedRows = await unitOfWork
                .Repository<Order>()
                .QueryAsync(x => x.Id == order.Id && x.Status == previousStatus)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(x => x.Status, target),
                    cancellationToken
                );
            if (transitionedRows != 1)
            {
                OrderStatus? persistedStatus = await unitOfWork
                    .Repository<Order>()
                    .QueryAsync(x => x.Id == order.Id)
                    .AsNoTracking()
                    .Select(x => (OrderStatus?)x.Status)
                    .SingleOrDefaultAsync(cancellationToken);
                await unitOfWork.RollbackAsync(cancellationToken);
                return persistedStatus == target
                    ? Result.Success()
                    : Failure("Order status changed concurrently.");
            }

            if (equipmentAction == EquipmentLifecycleAction.Claim)
            {
                int claimedRows = await unitOfWork
                    .Repository<Equipment>()
                    .QueryAsync(x =>
                        requestedEquipmentIds.Contains(x.Id)
                        && x.BranchId == order.BranchId
                        && x.Status == EquipmentStatus.Active
                        && !x.Using
                    )
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.Using, true),
                        cancellationToken
                    );
                if (claimedRows != requestedEquipmentIds.Length)
                    return await RollbackFailure(
                        CreateBadRequest("One or more equipments were claimed concurrently."),
                        cancellationToken
                    );
            }
            else if (equipmentAction == EquipmentLifecycleAction.Release)
            {
                long[] equipmentIds = order.OrderEquipments.Select(x => x.EquipmentId).ToArray();
                if (equipmentIds.Length != 0)
                    _ = await unitOfWork
                        .Repository<Equipment>()
                        .QueryAsync(x =>
                            equipmentIds.Contains(x.Id) && x.BranchId == order.BranchId
                        )
                        .ExecuteUpdateAsync(
                            setters => setters.SetProperty(x => x.Using, false),
                            cancellationToken
                        );
            }

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
                        CreateBadRequest(
                            materialResult.ErrorMessage ?? "Material consumption failed."
                        ),
                        cancellationToken
                    );
            }

            OrderTransitionResult applied = order.TransitionTo(
                target,
                request.Model.PaymentMethod,
                resolvedEquipments
            );
            if (applied != OrderTransitionResult.Applied)
                throw new InvalidOperationException(
                    $"Order transition changed after validation: {applied}."
                );

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

    private static Result Failure(string message) => Result.Failure(CreateBadRequest(message));

    private static BadRequestError CreateBadRequest(string message) =>
        new(
            message,
            Messager.Create<Order>().Message(MessageType.Valid).Negative().BuildMessage()
        );

    private static string GetTransitionError(OrderTransitionResult result) =>
        result switch
        {
            OrderTransitionResult.InvalidTransition => "Order status transition is not allowed.",
            OrderTransitionResult.PaymentMethodRequired =>
                "A valid payment method is required to complete an order.",
            OrderTransitionResult.PaymentMethodNotAllowed =>
                "Payment method is only allowed when completing an order.",
            OrderTransitionResult.EquipmentRequired =>
                "At least one equipment is required to start an order.",
            OrderTransitionResult.EquipmentNotAllowed =>
                "Equipment can only be selected when starting an order.",
            _ => "Order status transition is invalid.",
        };
}
