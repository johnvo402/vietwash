using System.Text.Json;
using Application.Common.HandleEventDomains.Orders;
using Application.Common.Interfaces.Services.DistributedCache;
using Domain.Aggregates.PubSubLogs;
using Domain.Events;
using Infrastructure.Data;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.IntegrationEvents;

public sealed class IntegrationOutboxDispatcher(
    TheDbContext db,
    IPubSubFactory queueFactory,
    ILogger logger
)
{
    public async Task<bool> DispatchOneAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        IntegrationOutbox? message;
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            message = (
                await db
                    .Set<IntegrationOutbox>()
                    .FromSqlInterpolated(
                        $"""
                        SELECT * FROM integration_outbox
                        WHERE delivered_at IS NULL AND next_attempt_at <= {now}
                          AND (locked_until IS NULL OR locked_until <= {now})
                        ORDER BY next_attempt_at, id LIMIT 1 FOR UPDATE SKIP LOCKED
                        """
                    )
                    .ToListAsync(cancellationToken)
            ).SingleOrDefault();
            if (message is null)
                return false;

            message.LeaseId = OutboxDeliveryPolicy.NewLeaseId();
            message.LockedUntil = OutboxDeliveryPolicy.LeaseUntil(now);
            message.Attempts = OutboxDeliveryPolicy.IncrementAttempts(message.Attempts);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        try
        {
            bool delivered = await PublishAsync(message);
            if (!delivered)
                throw new InvalidOperationException("Integration event was not acknowledged.");

            await OwnedLease(message).ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(x => x.DeliveredAt, DateTimeOffset.UtcNow)
                        .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null)
                        .SetProperty(x => x.LastError, (string?)null),
                cancellationToken
            );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            var retryAt = OutboxDeliveryPolicy.RetryAt(
                DateTimeOffset.UtcNow,
                message.Attempts
            );
            await OwnedLease(message).ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(x => x.NextAttemptAt, retryAt)
                        .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null)
                        .SetProperty(x => x.LastError, ex.GetType().Name),
                cancellationToken
            );
            logger.Warning(
                "Integration outbox retry scheduled. MessageId: {MessageId}, Attempt: {Attempt}, Failure: {Failure}",
                message.Id,
                message.Attempts,
                ex.GetType().Name
            );
        }
        finally
        {
            db.ChangeTracker.Clear();
        }

        return true;
    }

    private Task<bool> PublishAsync(IntegrationOutbox message) =>
        message.Topic switch
        {
            IntegrationOutbox.CreateFundTopic =>
                PublishPayload<CreateFundEvent>(message),
            IntegrationOutbox.EInvoiceTopic =>
                PublishPayload<EInvoiceOrderMessage>(message),
            _ => throw new InvalidOperationException(
                $"Unsupported integration outbox topic '{message.Topic}'."
            ),
        };

    private Task<bool> PublishPayload<T>(IntegrationOutbox message)
    {
        T payload =
            JsonSerializer.Deserialize<T>(message.Payload)
            ?? throw new InvalidOperationException("Invalid integration outbox payload.");
        return queueFactory
            .GetPubSub(PubSubType.Origin)
            .PublishAsync(payload, message.Topic);
    }

    private IQueryable<IntegrationOutbox> OwnedLease(IntegrationOutbox message) =>
        db.Set<IntegrationOutbox>()
            .Where(x =>
                x.Id == message.Id
                && x.LeaseId == message.LeaseId
                && x.DeliveredAt == null
            );
}
