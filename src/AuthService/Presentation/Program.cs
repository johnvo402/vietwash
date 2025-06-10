using Presentation.Extensions;
using Application;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Data;
using Infrastructure.Services.Hangfires;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Infrastructure.Services.BackgroundJobs;
using Contracts.Extensions;
using Contracts.Converters;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
services.AddScoped<JobScheduler>();
#region main dependencies
builder.AddConfiguration();
builder
    .Services.AddControllers()
    .AddJsonOptions(option =>
    {
        option.JsonSerializerOptions.Converters.Add(new DatetimeConverter());
        option.JsonSerializerOptions.Converters.Add(new DateTimeOffsetConvert());
        option.JsonSerializerOptions.Converters.Add(
            new Cysharp.Serialization.Json.UlidJsonConverter()
        );

    });
services.AddSwagger(configuration);
builder.AddOpenTelemetryTracing(configuration);
services.AddHealthChecks();
services.AddDatabaseHealthCheck(configuration);
#endregion

#region layers dependencies
services.AddInfrastructureDependencies(configuration, builder.Environment.EnvironmentName);
services.AddApplicationDependencies();
#endregion

try
{
    Log.Logger.Information("Application is starting....");
    var app = builder.Build();
    app.MapHealthChecks(
        "/api/health",
        new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
    );

    bool isDevelopment = app.Environment.IsDevelopment();
    bool isStaging = app.Environment.IsStaging();
    bool isProduction = app.Environment.IsProduction();

    #region seeding area
    var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var jobScheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();
    jobScheduler.ScheduleJobs();
    if (!isStaging && !isProduction)
    {
        await DbInitializer.InitializeAsync(serviceProvider);

    }
    #endregion

    app.UseHangfireDashboard(configuration);

    if (isDevelopment || isStaging)
    {
        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            x.RoutePrefix = "docs";
            x.ConfigObject.PersistAuthorization = true;
        });
    }

    app.UseAuthentication();
    app.CurrentUser();
    app.UseAuthorization();
    app.UseDetection();

    app.UseSerilogRequestLogging();
    app.LogContext();
    app.ExceptionHandler();
    app.BlackListContext();
    app.MapControllers();
    Log.Logger.Information(
        "Application is launching with {environment}",
        app.Environment.EnvironmentName
    );
    app.Run();
}
catch (Exception ex)
{
    Log.Logger.Fatal("Application has launched fail with error {error}", ex.Message);
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
