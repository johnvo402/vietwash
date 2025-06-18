using Contracts.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Contracts.Extensions;

public static class SerialogExtension
{
    public static void AddSerialogs(this WebApplicationBuilder builder)
    {
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration().ReadFrom.Configuration(
            builder.Configuration
        );

        SerilogSettings serilogSettings =
            builder.Configuration.GetSection(nameof(SerilogSettings)).Get<SerilogSettings>()
            ?? new();

        if (serilogSettings!.IsDistributeLog)
        {
            loggerConfiguration
                .WriteTo.Seq(serilogSettings.SeqUrl!)
                .WriteTo.File(
                    path: $"../../../logs/seq-{serilogSettings.ServiceName}/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1)
                );
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        builder.Host.UseSerilog(Log.Logger);

        builder.Services.AddSingleton(Log.Logger);
    }
}
