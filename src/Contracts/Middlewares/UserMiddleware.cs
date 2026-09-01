using Application.Common.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Contracts.Middlewares;

public class UserMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ICurrentAccount currentUser)
    {
        await currentUser.SetClaimPrinciple(context.User);

        currentUser.SetClientIp(context);

        await next.Invoke(context);
    }
}
