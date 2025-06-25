using Contracts.Routers;
using Contracts.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Contracts.Extensions;

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

        var openTelemetrySettings =
            configuration.GetSection(nameof(OpenTelemetrySettings)).Get<OpenTelemetrySettings>()
            ?? new();

        if (openTelemetrySettings.IsEnabled)
        {
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddService(
                    serviceName: openTelemetrySettings.ServiceName!,
                    serviceVersion: openTelemetrySettings.ServiceVersion ?? "unknown",
                    serviceInstanceId: Environment.MachineName
                );
            builder
                .Services.AddOpenTelemetry()
                .ConfigureResource(r =>
                    r.AddService(
                        serviceName: openTelemetrySettings.ServiceName!,
                        serviceVersion: openTelemetrySettings.ServiceVersion ?? "unknown",
                        serviceInstanceId: Environment.MachineName
                    )
                )
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
                            opt.Endpoint = new Uri(openTelemetrySettings.Endpoint!.ToString());
                            opt.Protocol = OtlpExportProtocol.Grpc;
                            opt.TimeoutMilliseconds = 300000;
                        });
                    }
                    else if (openTelemetrySettings.OtelpOption == OtelpOption.Console)
                    {
                        options.AddConsoleExporter();
                    }
                });
        }

        return builder.Services;
    }
}
