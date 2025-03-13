using Microsoft.AspNetCore.Builder;
using Contracts.Middlewares;

namespace Presentation.Extensions;

public static class MiddlewareRegisterExtension
{
    public static void ExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionHandler>();
    }

    public static void CurrentUser(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserMiddleware>();
    }

    public static void LogContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<LogContextMiddleware>();
    }

    public static void BlackListContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<BlackListMiddleware>();
    }
}
