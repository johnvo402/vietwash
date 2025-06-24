using Contracts.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Presentation.Extensions;

public static class MiddlewareRegisterExtension
{
    public static void CurrentUser(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserMiddleware>();
    }

    public static void BlackListContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<BlackListMiddleware>();
    }
}
