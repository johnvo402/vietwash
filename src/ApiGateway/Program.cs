using System.Threading.RateLimiting;
using ApiGateway.AppCheck.Extensions;
using ApiGateway.AppCheck.Models;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDetection();

// Thêm YARP
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000", "https://vietwash.vercel.app")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }
    );
});
var app = builder.Build();
app.UseCors("AllowFrontend");
app.UseApiKeyValidation();
app.UseRateLimiter();
app.MapGet("/", () => "Run oke!");

// Map YARP
app.MapReverseProxy();

app.Run();
