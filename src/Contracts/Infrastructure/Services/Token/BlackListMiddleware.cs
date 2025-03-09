
using Application.Common.Exceptions;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Infrastructure.Services.Token
{
    public class BlackListMiddleware(RequestDelegate _next, IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;

        public async Task Invoke(HttpContext context)
        {
            using var scope = serviceProvider.CreateScope();

            IBlacklistTokenService blacklistTokenService = scope.ServiceProvider.GetRequiredService<IBlacklistTokenService>();

            string? token = GetTokenFromHeader();
            if (token != null)
            {
                bool isBlacklisted = await blacklistTokenService.IsTokenBlacklistedAsync(token!);
                if (isBlacklisted)
                {

                    var httpContext = httpContextAccessor.HttpContext;
                    if (httpContext != null)
                    {
                        UnauthorizedException exception = new UnauthorizedException(Message.UNAUTHORIZED);
                        int statusCode = exception.HttpStatusCode;
                        httpContext.Response.StatusCode = statusCode;

                        ErrorResponse error =
                            new(exception.Message, nameof(UnauthorizedException), statusCode: statusCode);

                        await httpContext.Response.WriteAsJsonAsync(error, error.GetOptions());
                    }
                    return;
                }

            }
            await _next(context);
        }
        private string? GetTokenFromHeader()
        {
            var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                return authorizationHeader["Bearer ".Length..].Trim();
            }
            return null;
        }
    }
}