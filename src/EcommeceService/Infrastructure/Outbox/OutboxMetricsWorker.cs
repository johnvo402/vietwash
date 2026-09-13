using Contracts.Observability;
using Infrastructure.Data;
using Infrastructure.IntegrationEvents;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.Outbox;

public sealed class OutboxMetricsWorker(
    IServiceScopeFactory scopes,
    OutboxMetrics metrics,
    ILogger logger
) : BackgroundService
{
    private static readonly TimeSpan CollectionInterval = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Warning(
                    "Outbox metrics collection failed; retrying. Failure: {Failure}",
                    ex.GetType().Name
                );
            }

            try
            {
                await Task.Delay(CollectionInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task CollectAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TheDbContext>();

        var notification = await db
            .Set<NotificationOutbox>()
            .AsNoTracking()
            .Where(x => x.DeliveredAt == null)
            .GroupBy(_ => 1)
            .Select(group => new OutboxPendingStatus
            {
                Count = group.LongCount(),
                OldestCreatedAt = group.Min(x => x.CreatedAt),
            })
            .SingleOrDefaultAsync(cancellationToken);

        metrics.SetPending(
            OutboxMetricTypes.Notification,
            notification?.Count ?? 0,
            notification?.OldestCreatedAt
        );

        var integrations = await db
            .Set<IntegrationOutbox>()
            .AsNoTracking()
            .Where(x => x.DeliveredAt == null)
            .GroupBy(x => x.Topic)
            .Select(group => new OutboxPendingStatus
            {
                Topic = group.Key,
                Count = group.LongCount(),
                OldestCreatedAt = group.Min(x => x.CreatedAt),
            })
            .ToListAsync(cancellationToken);

        SetIntegrationPending(integrations, IntegrationOutbox.CreateFundTopic);
        SetIntegrationPending(integrations, IntegrationOutbox.EInvoiceTopic);
    }

    private void SetIntegrationPending(
        IReadOnlyList<OutboxPendingStatus> integrations,
        string topic
    )
    {
        var status = integrations.FirstOrDefault(item => item.Topic == topic);
        metrics.SetPending(
            OutboxMetricTypes.ForIntegrationTopic(topic),
            status?.Count ?? 0,
            status?.OldestCreatedAt
        );
    }

    private sealed class OutboxPendingStatus
    {
        public string? Topic { get; init; }
        public long Count { get; init; }
        public DateTimeOffset OldestCreatedAt { get; init; }
    }
}

internal static class OutboxMetricTypes
{
    public const string Notification = "notification";
    public const string Finance = "finance";
    public const string EInvoice = "einvoice";

    public static string ForIntegrationTopic(string topic) =>
        topic switch
        {
            IntegrationOutbox.CreateFundTopic => Finance,
            IntegrationOutbox.EInvoiceTopic => EInvoice,
            _ => "unknown",
        };
}
