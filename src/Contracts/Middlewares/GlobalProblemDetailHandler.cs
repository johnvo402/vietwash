using Application.Common.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Contracts.Middlewares
{
    public class GlobalProblemDetailHandler(
        IProblemDetailsService problemDetailsService,
        Serilog.ILogger logger
    ) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            IHttpActivityFeature? activityFeature =
                httpContext.Features.Get<IHttpActivityFeature>();
            string? traceId = activityFeature?.Activity?.TraceId.ToString();
            string? spanId = activityFeature?.Activity?.SpanId.ToString();
            logger.Error(
                "\n\n{exception} error's occured having tracing identifier [traceId:{traceId}, spanId:{spanId}]\nwith message '{Message}'\n{StackTrace}\n",
                exception.GetType().Name,
                traceId,
                spanId,
                exception.Message,
                exception.StackTrace?.TrimStart()
            );
            if (exception is ValidationException validationEx)
            {
                var validationError = new ValidationError(validationEx.Errors.ToList());

                var problemDetails = new ProblemDetails
                {
                    Title = validationError.Title,
                    Type = validationError.Type,
                    Status = validationError.Status,
                    Detail = validationError.Detail,
                };
                problemDetails.Extensions["invalidParams"] = validationError.InvalidParams;

                httpContext.Response.StatusCode = validationError.Status;
                httpContext.Response.ContentType = "application/problem+json";

                await httpContext.Response.WriteAsJsonAsync(problemDetails);
                return true;
            }

            int code = StatusCodes.Status500InternalServerError;
            httpContext.Response.StatusCode = code;

            ProblemDetails problemDetail = new()
            {
                Status = code,
                Title = "An Error has occured",
                Detail = exception.Message,
                Type = exception.GetType().Name,
            };

            return await problemDetailsService.TryWriteAsync(
                new()
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetail,
                    Exception = exception,
                }
            );
        }
    }
}
