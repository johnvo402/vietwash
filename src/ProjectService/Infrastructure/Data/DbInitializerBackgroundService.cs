using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class DbInitializerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DbInitializerBackgroundService> _logger;
    private readonly IHostEnvironment _env;

    public DbInitializerBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DbInitializerBackgroundService> logger,
        IHostEnvironment env
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _env = env;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_env.IsProduction())
            return;

        await Task.Delay(TimeSpan.FromSeconds(35), stoppingToken);

        stoppingToken.ThrowIfCancellationRequested();

        try
        {
            _logger.LogInformation("Start DbInitializer...");

            using var scope = _serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;

            await DbInitializer.InitializeAsync(sp);

            _logger.LogInformation("DbInitializer finished.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DbInitializer failed.");
        }
    }
}
