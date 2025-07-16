using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using ApiGateway.AppCheck.Models;
using ApiGateway.AppCheck.Services;
using Microsoft.AspNetCore.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDetection();
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(static builderContext =>
    {
        if (
            builderContext.Route?.Metadata != null
            && builderContext.Route.Metadata.TryGetValue(
                "ApiKeyRequired",
                out var apiKeyRequiredValue
            )
            && bool.TryParse(apiKeyRequiredValue?.ToString(), out var apiKeyRequired)
            && apiKeyRequired
        )
        {
            builderContext.AddRequestTransform(static async context =>
            {
                var validator =
                    context.HttpContext.RequestServices.GetRequiredService<IApiKeyValidator>();
                var isValid = await validator.ValidateAsync(context.HttpContext);

                if (!isValid)
                {
                    context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.HttpContext.Response.ContentType = "application/json";
                    var response = JsonSerializer.Serialize(
                        new { error = "Invalid ApiKey or Platform." }
                    );
                    await context.HttpContext.Response.WriteAsync(response);
                    return;
                }
            });
        }
    });

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        "customPolicy",
        opt =>
        {
            opt.PermitLimit = 4;
            opt.Window = TimeSpan.FromSeconds(12);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        }
    );
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "https://vietwash.vercel.app",
                    "https://api-app.payos.vn"
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.MapGet("/", () => "Run oke!");

app.MapReverseProxy();

app.Run();
