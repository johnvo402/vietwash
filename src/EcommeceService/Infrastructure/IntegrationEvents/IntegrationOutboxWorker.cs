using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Infrastructure.IntegrationEvents;

public sealed class IntegrationOutboxWorker(IServiceScopeFactory scopes, ILogger logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = scopes.CreateAsyncScope();
                if (
                    await scope
                        .ServiceProvider.GetRequiredService<IntegrationOutboxDispatcher>()
                        .DispatchOneAsync(stoppingToken)
                )
                    continue;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.Warning(
                    "Integration outbox scan failed; retrying. Failure: {Failure}",
                    ex.GetType().Name
                );
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
