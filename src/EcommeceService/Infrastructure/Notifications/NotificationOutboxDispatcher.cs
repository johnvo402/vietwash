using System.Globalization;
using System.Text.Json;
using Contracts.Application.Common.Interfaces.Services.Notifications;
using Domain.Aggregates.Users;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Notification_Grpc;
using Serilog;

namespace Infrastructure.Notifications;

public sealed class NotificationOutboxDispatcher(TheDbContext db, INotificationGrpc notification, ILogger logger)
{
    public async Task<bool> DispatchOneAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        NotificationOutbox? message;
        // SKIP LOCKED + a persisted lease supports multiple workers and process crashes.
        // No transaction/row lock remains open during the network call.
        await using (var transaction = await db.Database.BeginTransactionAsync(cancellationToken))
        {
            message = (await db.Set<NotificationOutbox>().FromSqlInterpolated($"""
                SELECT * FROM notification_outbox
                WHERE delivered_at IS NULL AND next_attempt_at <= {now}
                  AND (locked_until IS NULL OR locked_until <= {now})
                ORDER BY next_attempt_at, id LIMIT 1 FOR UPDATE SKIP LOCKED
                """).ToListAsync(cancellationToken)).SingleOrDefault();
            if (message == null) return false;
            message.LeaseId = Guid.NewGuid();
            message.LockedUntil = now.AddMinutes(1);
            message.Attempts = Math.Min(message.Attempts + 1, 1000000);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            var payload = JsonSerializer.Deserialize<ProcessedOrderNotification>(message.Payload)
                ?? throw new InvalidOperationException("Invalid notification outbox payload");
            var branch = await db.Set<BranchUser>().AsNoTracking()
                .Where(x => x.BranchId == payload.BranchId).Select(x => x.BranchName).FirstOrDefaultAsync(timeout.Token);
            var request = new SendNotificationRequest
            {
                MessageId = message.Id,
                TemplateId = "laundry_processed",
                Time = payload.OccurredAt.ToString("O", CultureInfo.InvariantCulture),
            };
            request.UserIds.Add(payload.CustomerId.ToString(CultureInfo.InvariantCulture));
            request.Parameters["order_code"] = payload.OrderCode;
            request.Parameters["branch_name"] = branch ?? $"#{payload.BranchId}";
            request.Data["order_id"] = payload.OrderId.ToString(CultureInfo.InvariantCulture);
            request.Data["publicId"] = payload.PublicId;
            if (!await notification.SendNotifyAsync(request, timeout.Token))
                throw new InvalidOperationException("Notification receiver did not acknowledge persistence");

            await OwnedLease(message).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.DeliveredAt, DateTimeOffset.UtcNow)
                .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null)
                .SetProperty(x => x.LastError, (string?)null), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Shutdown leaves the lease recoverable; an acknowledged-but-unmarked
            // delivery is safe to retry because the receiver persists MessageId.
            throw;
        }
        catch (Exception ex)
        {
            var retryAt = DateTimeOffset.UtcNow.AddSeconds(Math.Min(3600, Math.Pow(2, Math.Min(12, message.Attempts))));
            // No payload, customer data, credentials or provider responses in persisted errors.
            await OwnedLease(message).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.NextAttemptAt, retryAt)
                .SetProperty(x => x.LockedUntil, (DateTimeOffset?)null)
                .SetProperty(x => x.LastError, ex.GetType().Name), cancellationToken);
            logger.Warning("Notification outbox retry scheduled. MessageId: {MessageId}, Attempt: {Attempt}, Failure: {Failure}",
                message.Id, message.Attempts, ex.GetType().Name);
        }
        finally { db.ChangeTracker.Clear(); }
        return true;
    }

    private IQueryable<NotificationOutbox> OwnedLease(NotificationOutbox message) =>
        db.Set<NotificationOutbox>().Where(x => x.Id == message.Id && x.LeaseId == message.LeaseId && x.DeliveredAt == null);
}
