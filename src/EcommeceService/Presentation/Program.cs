using System.Text.Json.Serialization;
using Application;
using Contracts.Converters;
using Contracts.Extensions;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Services.BackgroundJobs;
using Infrastructure.Services.Hangfires;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Presentation.Extensions;
using Serilog;

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
        option.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
services.AddErrorDetails();
services.AddSwagger(configuration);
builder.AddOpenTelemetryTracing(configuration);
builder.AddSerialogs();
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

    bool isDevelopment = app.Environment.IsDevelopment();
    bool isStaging = app.Environment.IsStaging();
    bool isProduction = app.Environment.IsProduction();

    #region job
    var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;
    var jobScheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();
    jobScheduler.ScheduleJobs();
    #endregion

    app.UseHangfireDashboard(configuration);

    if (isDevelopment)
    {
        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            x.RoutePrefix = "docs";
            x.ConfigObject.PersistAuthorization = true;
        });
        app.AddLog(Log.Logger, "docs", "/api/health");
    }
    if (isDevelopment)
    {
        app.UseDeveloperExceptionPage();
    }
    app.UseStatusCodePages();
    app.UseExceptionHandler();
    app.UseAuthentication();
    app.CurrentUser();
    app.UseAuthorization();
    app.UseDetection();
    app.UseSerilogRequestLogging();
    app.BlackListContext();
    app.MapControllers();
    app.ApplyMigrations();
    app.MapHealthChecks(
        "/api/health",
        new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
    );

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
