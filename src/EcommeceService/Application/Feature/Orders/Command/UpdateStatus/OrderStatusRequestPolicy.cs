using Application.Common.Errors;
using Application.Common.Interfaces.Services;
using Application.Feature.Orders.Common;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;

namespace Application.Feature.Orders.Command.UpdateStatus;

internal static class OrderStatusRequestPolicy
{
    public static ErrorDetails? ValidateActor(
        UpdateStatusCommand request,
        ICurrentAccount currentAccount
    ) =>
        !request.IsVerifiedPayOsWebhook
        && !OrderActorAccess.IsStaffSide(currentAccount.Session?.Role)
            ? new ForbiddenError(Message.FORBIDDEN)
            : null;

    public static ErrorDetails? ValidateOrder(
        UpdateStatusCommand request,
        ICurrentAccount currentAccount,
        Order order,
        OrderStatus target
    )
    {
        if (
            !request.IsVerifiedPayOsWebhook
            && !OrderActorAccess.CanOperateOrder(
                currentAccount.Session?.Role,
                currentAccount.Session?.Branches,
                order.BranchId
            )
        )
            return new ForbiddenError(Message.FORBIDDEN);

        if (
            !request.IsVerifiedPayOsWebhook
            && order.Status == OrderStatus.Processed
            && target == OrderStatus.Completed
            && request.Model.PaymentMethod == PaymentMethod.Card
        )
            return BadRequest("Card payments must be completed through PayOS.");

        if (
            request.ExpectedPaymentAmount.HasValue
            && (
                !PayOsOrderPolicy.TryGetAmount(order.Total, out int authoritativeAmount)
                || authoritativeAmount != request.ExpectedPaymentAmount.Value
            )
        )
            return BadRequest("Payment amount does not match the order total.");

        return null;
    }

    public static CancellationPreparation PrepareCancellation(
        UpdateStatusCommand request,
        ICurrentAccount currentAccount,
        OrderStatus target,
        TimeProvider timeProvider
    )
    {
        if (target != OrderStatus.Cancelled)
            return string.IsNullOrWhiteSpace(request.Model.CancellationReason)
                ? CancellationPreparation.Success(null)
                : CancellationPreparation.Failure(
                    BadRequest("Cancellation reason is only allowed when cancelling an order.")
                );

        if (currentAccount.Id is not long cancelledBy || cancelledBy <= 0)
            return CancellationPreparation.Failure(new UnauthorizedError(Message.UNAUTHORIZED));

        string reason = request.Model.CancellationReason?.Trim() ?? string.Empty;
        if (
            reason.Length
            is < OrderCancellation.MinimumReasonLength or > OrderCancellation.MaximumReasonLength
        )
            return CancellationPreparation.Failure(
                BadRequest(
                    $"Cancellation reason must contain between {OrderCancellation.MinimumReasonLength} and {OrderCancellation.MaximumReasonLength} characters."
                )
            );

        return CancellationPreparation.Success(
            OrderCancellation.Create(timeProvider.GetUtcNow(), cancelledBy, reason)
        );
    }

    public static BadRequestError BadRequest(string message) =>
        new(message, Messager.Create<Order>().Message(MessageType.Valid).Negative().BuildMessage());

    public static string GetTransitionError(OrderTransitionResult result) =>
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
            OrderTransitionResult.CancellationRequired =>
                "Cancellation metadata is required to cancel an order.",
            OrderTransitionResult.CancellationNotAllowed =>
                "Cancellation metadata is only allowed when cancelling an order.",
            _ => "Order status transition is invalid.",
        };
}

internal sealed record CancellationPreparation(
    OrderCancellation? Cancellation,
    ErrorDetails? Error
)
{
    public static CancellationPreparation Success(OrderCancellation? cancellation) =>
        new(cancellation, null);

    public static CancellationPreparation Failure(ErrorDetails error) => new(null, error);
}
