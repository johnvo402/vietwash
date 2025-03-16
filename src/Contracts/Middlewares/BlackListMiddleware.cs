using Application.Common.Exceptions;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Dtos.Models;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Middlewares
{
    public class BlackListMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        public BlackListMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var blacklistTokenService = scope.ServiceProvider.GetRequiredService<ITokenSecurityService>();

            var (token, nonce, signature, timestamp) = GetTokenFromHeader(context);

            if (token is null)
            {
                await _next(context);
                return;
            }

            var decodeToken = blacklistTokenService.DecodeToken(token);
            bool isBlacklisted = await blacklistTokenService.IsTokenBlacklistedAsync(decodeToken.FamilyId!);
            if (isBlacklisted)
            {
                await ReturnUnauthorizedAsync(context);
                return;
            }

            await _next(context);
        }

        private (string?, string?, string?, string?) GetTokenFromHeader(HttpContext context)
        {
            var headers = context.Request.Headers;

            var authorizationHeader = headers["Authorization"].FirstOrDefault();
            var nonceHeader = headers["X-Nonce"].FirstOrDefault();
            var signatureHeader = headers["X-Signature"].FirstOrDefault();
            var timestampHeader = headers["X-Timestamp"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorizationHeader["Bearer ".Length..].Trim();
                return (token, nonceHeader, signatureHeader, timestampHeader);
            }

            return (null, null, null, null);
        }


        private async Task ReturnUnauthorizedAsync(HttpContext context)
        {
            var exception = new UnauthorizedException(Message.UNAUTHORIZED);
            int statusCode = exception.HttpStatusCode;
            context.Response.StatusCode = statusCode;

            var error = new ErrorResponse(exception.Message, nameof(UnauthorizedException), statusCode: statusCode);

            await context.Response.WriteAsJsonAsync(error, error.GetOptions());
        }
    }
}
