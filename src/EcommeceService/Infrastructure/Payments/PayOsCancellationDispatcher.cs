using Application.Feature.Orders.Command.UpdateStatus;
using Application.Feature.Orders.Queries.GetLinkPayment;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Vouchers;
using Infrastructure.Data;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Payments;

public sealed class PayOsCancellationDispatcher(
    TheDbContext db,
    IOrderPaymentLinkClient paymentClient,
    ILogger logger
)
{
    public async Task<bool> DispatchOneAsync(CancellationToken cancellationToken)
    {
        PayOsCancellationRequest? request = await ClaimAsync(cancellationToken);
        if (request is null)
            return false;

        try
        {
            ProcessedOrderPaymentCancellationResult result =
                await ProcessedOrderPaymentCancellation.EnsureSafeAsync(
                    paymentClient,
                    request.OrderId,
                    request.Reason,
                    logger
                );
            cancellationToken.ThrowIfCancellationRequested();

            if (result.IsSafe)
                await FinalizeAsync(request, result.State, cancellationToken);
            else
                await FailAsync(
                    request,
                    result.State,
                    result.ErrorMessage ?? "The payment link could not be cancelled safely.",
                    retryable: result.State != ProcessedOrderPaymentState.Paid,
                    cancellationToken
                );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The persisted lease expires and makes an interrupted attempt recoverable.
            throw;
        }
        catch (Exception ex)
        {
            db.ChangeTracker.Clear();
            await FailAsync(
                request,
                ProcessedOrderPaymentState.Unknown,
                ex.GetType().Name,
                retryable: true,
                cancellationToken
            );
            logger.Warning(
                "PayOS cancellation retry scheduled. OrderId: {OrderId}, Attempt: {Attempt}, Failure: {Failure}",
                request.OrderId,
                request.Attempts,
                ex.GetType().Name
            );
        }
        finally
        {
            db.ChangeTracker.Clear();
        }

        return true;
    }

    private async Task<PayOsCancellationRequest?> ClaimAsync(
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        PayOsCancellationRequest? request = (
            await db.Set<PayOsCancellationRequest>().FromSqlInterpolated($"""
                SELECT * FROM payos_cancellation_request
                WHERE (
                    (state IN ('Pending', 'Failed') AND next_attempt_at <= {now})
                    OR (state = 'Processing' AND locked_until <= {now})
                )
                ORDER BY next_attempt_at NULLS LAST, order_id
                LIMIT 1 FOR UPDATE SKIP LOCKED
                """).ToListAsync(cancellationToken)
        ).SingleOrDefault();
        if (request is null)
            return null;

        request.MarkProcessing(
            OutboxDeliveryPolicy.NewLeaseId(),
            OutboxDeliveryPolicy.LeaseUntil(now),
            OutboxDeliveryPolicy.IncrementAttempts(request.Attempts)
        );
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        db.ChangeTracker.Clear();
        return request;
    }

    private async Task FinalizeAsync(
        PayOsCancellationRequest claimed,
        ProcessedOrderPaymentState providerState,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        PayOsCancellationRequest? request = (
            await db.Set<PayOsCancellationRequest>().FromSqlInterpolated($"""
                SELECT * FROM payos_cancellation_request
                WHERE order_id = {claimed.OrderId}
                  AND lease_id = {claimed.LeaseId}
                  AND state = 'Processing'
                FOR UPDATE
                """).ToListAsync(cancellationToken)
        ).SingleOrDefault();
        if (request is null)
            return;

        Order? order = (
            await db.Set<Order>().FromSqlInterpolated($"""
                SELECT * FROM "order" WHERE id = {request.OrderId} FOR UPDATE
                """).ToListAsync(cancellationToken)
        ).SingleOrDefault();
        if (order is null)
        {
            request.MarkFailed(
                providerState.ToString(),
                "Order no longer exists.",
                nextAttemptAt: null
            );
        }
        else if (order.Status == OrderStatus.Cancelled)
        {
            request.MarkCompleted(DateTimeOffset.UtcNow, providerState.ToString());
        }
        else if (order.Status != OrderStatus.Processed)
        {
            request.MarkFailed(
                providerState.ToString(),
                $"Order is already {order.Status} and was not cancelled.",
                nextAttemptAt: null
            );
        }
        else if (await db.Set<VoucherUsage>().AnyAsync(
            x => x.OrderId == order.Id,
            cancellationToken
        ))
        {
            request.MarkFailed(
                providerState.ToString(),
                "Completed voucher usage already exists; the order was not cancelled.",
                nextAttemptAt: null
            );
        }
        else
        {
            if (order.CustomerId is long customerId && order.VoucherId is long voucherId)
                _ = await db.Set<VoucherCustomer>()
                    .Where(x =>
                        x.VoucherId == voucherId && x.CustomerId == customerId && x.IsUsed
                    )
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(x => x.IsUsed, false),
                        cancellationToken
                    );

            OrderTransitionResult applied = order.TransitionTo(
                OrderStatus.Cancelled,
                orderEquipments: [],
                cancellation: OrderCancellation.Create(
                    request.RequestedAt,
                    request.CancelledBy,
                    request.Reason
                )
            );
            if (applied != OrderTransitionResult.Applied)
                throw new InvalidOperationException(
                    $"Order cancellation transition changed after provider coordination: {applied}."
                );

            order.Version = checked(order.Version + 1);
            request.MarkCompleted(DateTimeOffset.UtcNow, providerState.ToString());
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private Task<int> FailAsync(
        PayOsCancellationRequest request,
        ProcessedOrderPaymentState providerState,
        string error,
        bool retryable,
        CancellationToken cancellationToken
    )
    {
        var providerStateName = providerState.ToString();
        DateTimeOffset? nextAttemptAt = retryable
            ? OutboxDeliveryPolicy.RetryAt(DateTimeOffset.UtcNow, request.Attempts)
            : null;

        return db.Set<PayOsCancellationRequest>()
            .Where(x =>
                x.OrderId == request.OrderId
                && x.LeaseId == request.LeaseId
                && x.State == PayOsCancellationState.Processing
            )
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.State, PayOsCancellationState.Failed)
                    .SetProperty(x => x.ProviderState, providerStateName)
                    .SetProperty(x => x.LastError, error)
                    .SetProperty(x => x.NextAttemptAt, nextAttemptAt)
                    .SetProperty(x => x.LeaseId, (Guid?)null)
                    .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null),
                cancellationToken
            );
    }
}
