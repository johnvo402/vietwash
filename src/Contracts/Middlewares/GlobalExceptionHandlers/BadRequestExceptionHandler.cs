using System.Diagnostics;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Contracts.Middlewares.GlobalExceptionHandlers;

public class BadRequestExceptionHandler : IHandlerException<BadRequestException>
{
    public async Task Handle(HttpContext httpContext, Exception ex)
    {
        var exception = (BadRequestException)ex;

        httpContext.Response.StatusCode = exception.HttpStatusCode;

        ErrorResponse error = new(
            exception.Errors,
            exception.GetType().Name,
            exception.Message,
            new()
            {
                TraceId = Activity.Current?.Context.TraceId.ToString(),
                SpanId = Activity.Current?.Context.SpanId.ToString(),
            }
        );

        await httpContext.Response.WriteAsJsonAsync(error, error.GetOptions());
    }
}
