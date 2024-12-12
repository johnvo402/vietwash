using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
// Thêm YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Thêm Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"] ?? "http://localhost:6379"));
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("customPolicy", opt =>
    {
        opt.PermitLimit = 4;
        opt.Window = TimeSpan.FromSeconds(12);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://d3c2-2001-ee0-5305-c4c0-4c04-8d23-43ae-b514.ngrok-free.app",
                "http://localhost:5000", "http://gateway-api:5000"
            ) // Add your ngrok URL
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");

app.Use(async (context, next) =>
{
    // Don't cache if request has query parameters
    if (context.Request.QueryString.HasValue)
    {
        await next();
        return;
    }

    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    string cacheKey = $"YARP:{context.Request.Path}";

    // Check cache for GET requests
    if (context.Request.Method == HttpMethod.Get.Method)
    {
        var cachedResponse = await db.StringGetAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedResponse))
        {
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cachedResponse.ToString());
            return;
        }
    }

    // Continue pipeline with stream capture
    var originalBodyStream = context.Response.Body;
    using var newResponseStream = new MemoryStream();
    context.Response.Body = newResponseStream;

    await next();

    // Always copy response back, but only cache 200 OK GET requests
    newResponseStream.Seek(0, SeekOrigin.Begin);
    string responseText = new StreamReader(newResponseStream).ReadToEnd();

    if (context.Response.StatusCode == 200 && context.Request.Method == HttpMethod.Get.Method)
    {
        await db.StringSetAsync(cacheKey, responseText, TimeSpan.FromMinutes(10));
    }

    newResponseStream.Seek(0, SeekOrigin.Begin);
    await newResponseStream.CopyToAsync(originalBodyStream);
    context.Response.Body = originalBodyStream;
});
app.UseRateLimiter();
// Map YARP
app.MapReverseProxy();

app.Run();
