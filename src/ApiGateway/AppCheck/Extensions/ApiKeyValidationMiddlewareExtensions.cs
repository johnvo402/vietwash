using ApiGateway.AppCheck.Middlewares;

namespace ApiGateway.AppCheck.Extensions;

public static class ApiKeyValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyValidation(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyValidationMiddleware>();
    }
}
