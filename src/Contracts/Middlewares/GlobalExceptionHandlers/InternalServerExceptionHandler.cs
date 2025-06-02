using System.Diagnostics;
using JohnChum.SharedKernel.SpecificationQuery.LHS.ApiWrapper;
using Microsoft.AspNetCore.Http;

namespace Contracts.Middlewares.GlobalExceptionHandlers;

public class InternalServerExceptionHandler() : IHandlerException
{
    public async Task Handle(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var error = new ErrorResponse(
            ex.Message,
            trace: new()
            {
                TraceId = Activity.Current?.Context.TraceId.ToString(),
                SpanId = Activity.Current?.Context.SpanId.ToString(),
            }
        );

        await httpContext.Response.WriteAsJsonAsync(error, error.GetOptions());
    }
}
