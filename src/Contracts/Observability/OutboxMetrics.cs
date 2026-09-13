using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace Contracts.Observability;

public sealed class OutboxMetrics : IDisposable
{
    public const string MeterName = "VietWash.Outbox";

    private readonly ConcurrentDictionary<string, PendingSnapshot> pending = new();
    private readonly Meter meter = new(MeterName);
    private readonly Counter<long> successfulDeliveries;
    private readonly Counter<long> failedDeliveries;
    private readonly Counter<long> retries;

    public OutboxMetrics()
    {
        successfulDeliveries = meter.CreateCounter<long>(
            "vietwash.outbox.deliveries.success",
            "{message}"
        );
        failedDeliveries = meter.CreateCounter<long>(
            "vietwash.outbox.deliveries.failed",
            "{message}"
        );
        retries = meter.CreateCounter<long>("vietwash.outbox.retries", "{message}");
        meter.CreateObservableGauge(
            "vietwash.outbox.pending_messages",
            ObservePendingMessages,
            "{message}"
        );
        meter.CreateObservableGauge(
            "vietwash.outbox.oldest_pending_age",
            ObserveOldestPendingAge,
            "s"
        );
    }

    public void SetPending(string outboxType, long count, DateTimeOffset? oldestCreatedAt) =>
        pending[outboxType] = new(count, oldestCreatedAt);

    public void RecordSuccessfulDelivery(string outboxType) =>
        successfulDeliveries.Add(1, OutboxTypeTag(outboxType));

    public void RecordFailedDelivery(string outboxType) =>
        failedDeliveries.Add(1, OutboxTypeTag(outboxType));

    public void RecordRetry(string outboxType) =>
        retries.Add(1, OutboxTypeTag(outboxType));

    public void Dispose() => meter.Dispose();

    private IEnumerable<Measurement<long>> ObservePendingMessages() =>
        pending.Select(item => new Measurement<long>(
            item.Value.Count,
            OutboxTypeTag(item.Key)
        ));

    private IEnumerable<Measurement<double>> ObserveOldestPendingAge()
    {
        var now = DateTimeOffset.UtcNow;
        return pending.Select(item => new Measurement<double>(
            item.Value.OldestCreatedAt is DateTimeOffset oldest
                ? Math.Max(0, (now - oldest).TotalSeconds)
                : 0,
            OutboxTypeTag(item.Key)
        ));
    }

    private static KeyValuePair<string, object?> OutboxTypeTag(string outboxType) =>
        new("outbox_type", outboxType);

    private sealed record PendingSnapshot(long Count, DateTimeOffset? OldestCreatedAt);
}
