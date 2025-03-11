using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Presentation.Settings;
using Serilog;
using Contracts.Routers;
namespace Presentation.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryTracing(
    this WebApplicationBuilder builder,
    IConfiguration configuration
)
    {
        builder.Services.Configure<OpenTelemetrySettings>(
            configuration.GetSection(nameof(OpenTelemetrySettings))
        );

       

        var openTelemetrySettings = configuration.GetSection(nameof(OpenTelemetrySettings))
            .Get<OpenTelemetrySettings>() ?? new();

        builder.Services.Configure<OtlpExporterOptions>(o => o.Headers = $"x-otlp-api-key={openTelemetrySettings.OtelApiKey!}");
        if (openTelemetrySettings.IsEnabled)
        {
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: openTelemetrySettings.ServiceName!,
                    serviceVersion: openTelemetrySettings.ServiceVersion ?? "unknown",
                    serviceInstanceId: Environment.MachineName
                );

            builder.Services
                .AddOpenTelemetry()
                .ConfigureResource(r =>
                r.AddService(
                    serviceName: openTelemetrySettings.ServiceName!,
                    serviceVersion: openTelemetrySettings.ServiceVersion ?? "unknown",
                    serviceInstanceId: Environment.MachineName
                ))
                .WithTracing(options =>
                {
                    options
                        .AddSource(openTelemetrySettings.ActivitySourceName!)
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation(opt =>
                        {
                            opt.Filter = context =>
                                !string.IsNullOrEmpty(context.Request.Path.Value)
                                && context.Request.Path.Value.Contains(
                                    RouterBase.prefix.Replace("/", string.Empty),
                                    StringComparison.InvariantCulture
                                );
                            opt.EnrichWithHttpRequest = (activity, httpRequest) =>
                                activity.SetTag("requestProtocol", httpRequest.Protocol);
                            opt.EnrichWithHttpResponse = (activity, httpResponse) =>
                                activity.SetTag("responseLength", httpResponse.ContentLength);
                            opt.RecordException = true;
                            opt.EnrichWithException = (activity, exception) =>
                            {
                                activity.SetTag("exceptionType", exception?.GetType().ToString());
                                activity.SetTag("stackTrace", exception?.StackTrace);
                            };
                        })
                        .AddEntityFrameworkCoreInstrumentation(opt =>
                        {
                            opt.SetDbStatementForText = true;
                            opt.SetDbStatementForStoredProcedure = true;
                            opt.EnrichWithIDbCommand = (activity, command) =>
                            {
                                var stateDisplayName = $"{command.CommandType} main";
                                activity.DisplayName = stateDisplayName;
                                activity.SetTag("db.name", stateDisplayName);
                            };
                        });

                    if (openTelemetrySettings.OtelpOption == OtelpOption.DistributedServer)
                    {
                        options.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(openTelemetrySettings.Otelp!.ToString());
                            opt.Protocol = OtlpExportProtocol.Grpc;
                            opt.TimeoutMilliseconds = 300000;
                        });
                    }
                    else if (openTelemetrySettings.OtelpOption == OtelpOption.Console)
                    {
                        options.AddConsoleExporter();
                    }
                })
                .WithMetrics(options =>
                {
                    options.AddMeter("CustomMetrics")
                        .AddRuntimeInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation();

                    if (openTelemetrySettings.OtelpOption == OtelpOption.DistributedServer)
                    {
                        options.AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(openTelemetrySettings.Otelp!.ToString());
                            opt.Protocol = OtlpExportProtocol.Grpc;
                            opt.TimeoutMilliseconds = 300000;
                        });
                    }
                    else if (openTelemetrySettings.OtelpOption == OtelpOption.Console)
                    {
                        options.AddConsoleExporter();
                    }
                });

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = openTelemetrySettings.Otelp!;
                    options.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
                    options.Headers.Add("x-otlp-api-key", openTelemetrySettings.OtelApiKey!);

                    // Dùng chung Resource Attributes
                    foreach (var kvp in resourceBuilder.Build().Attributes)
                    {
                        options.ResourceAttributes[kvp.Key] = kvp.Value?.ToString()!;
                    }
                });

            Log.Logger = loggerConfiguration.CreateLogger();
            builder.Host.UseSerilog(Log.Logger);
            builder.Services.AddSingleton(Log.Logger);
        }

        return builder.Services;
    }
}
