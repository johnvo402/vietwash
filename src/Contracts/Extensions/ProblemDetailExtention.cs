using Contracts.Middlewares;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Extensions
{
    public static class ProblemDetailExtention
    {
        public static IServiceCollection AddErrorDetails(this IServiceCollection services)
        {
            return services
                .AddProblemDetails(options =>
                {
                    options.CustomizeProblemDetails = context =>
                    {
                        context.ProblemDetails.Instance =
                            $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                        context.ProblemDetails.Extensions.TryAdd(
                            "requestId",
                            context.HttpContext.TraceIdentifier
                        );
                        IHttpActivityFeature? activityFeature =
                            context.HttpContext.Features.Get<IHttpActivityFeature>();

                        context.ProblemDetails.Extensions.TryAdd(
                            "traceId",
                            activityFeature?.Activity?.TraceId.ToString()
                        );
                        context.ProblemDetails.Extensions.TryAdd(
                            "spanId",
                            activityFeature?.Activity?.SpanId.ToString()
                        );
                    };
                })
                .AddExceptionHandler<GlobalProblemDetailHandler>();
        }
    }
}
