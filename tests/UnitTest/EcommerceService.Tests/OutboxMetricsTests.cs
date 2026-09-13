using System.Diagnostics.Metrics;
using Contracts.Observability;

namespace EcommerceService.Tests;

public class OutboxMetricsTests
{
    [Fact]
    public void EmitsDeliveryRetryAndBacklogMetricsWithOutboxType()
    {
        var measurements = new List<CapturedMeasurement>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, currentListener) =>
        {
            if (instrument.Meter.Name == OutboxMetrics.MeterName)
                currentListener.EnableMeasurementEvents(instrument);
        };
        listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
            measurements.Add(new(instrument.Name, value, GetOutboxType(tags))));
        listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) =>
            measurements.Add(new(instrument.Name, value, GetOutboxType(tags))));
        listener.Start();

        using var metrics = new OutboxMetrics();
        metrics.RecordSuccessfulDelivery("notification");
        metrics.RecordFailedDelivery("finance");
        metrics.RecordRetry("einvoice");
        metrics.SetPending("notification", 1, DateTimeOffset.UtcNow.AddMinutes(-1));
        metrics.SetPending("finance", 3, DateTimeOffset.UtcNow.AddMinutes(-5));
        metrics.SetPending("einvoice", 0, null);
        listener.RecordObservableInstruments();

        Assert.Contains(measurements, item =>
            item is { Name: "vietwash.outbox.deliveries.success", Value: 1, OutboxType: "notification" });
        Assert.Contains(measurements, item =>
            item is { Name: "vietwash.outbox.deliveries.failed", Value: 1, OutboxType: "finance" });
        Assert.Contains(measurements, item =>
            item is { Name: "vietwash.outbox.retries", Value: 1, OutboxType: "einvoice" });
        Assert.Contains(measurements, item =>
            item is { Name: "vietwash.outbox.pending_messages", Value: 3, OutboxType: "finance" });
        Assert.Contains(measurements, item =>
            item.Name == "vietwash.outbox.oldest_pending_age"
            && item.OutboxType == "finance"
            && item.Value is >= 290 and <= 310);
    }

    private static string? GetOutboxType(
        ReadOnlySpan<KeyValuePair<string, object?>> tags
    )
    {
        foreach (var tag in tags)
            if (tag.Key == "outbox_type")
                return tag.Value?.ToString();

        return null;
    }

    private sealed record CapturedMeasurement(string Name, double Value, string? OutboxType);
}
