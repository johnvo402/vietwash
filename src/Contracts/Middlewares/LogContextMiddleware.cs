using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Contracts.Middlewares;

public class LogContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.TraceId.ToString();
        var spanId = Activity.Current?.SpanId.ToString();

        using (LogContext.PushProperty("trace-id", traceId))
        using (LogContext.PushProperty("span-id", spanId))
        {
            context.Response.Headers["trace-id"] = traceId;
            context.Response.Headers["span-id"] = spanId;

            await next(context);
        }
    }
}
