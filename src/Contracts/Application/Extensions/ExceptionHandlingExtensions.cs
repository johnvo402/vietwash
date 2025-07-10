using Application.Common.Errors;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Common.Extensions;

public static class ExceptionHandlingExtensions
{
    public static void UseCustomExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = feature?.Error;

                if (exception is ValidationException validationEx)
                {
                    var validationError = new ValidationError(validationEx.Errors.ToList());

                    var problemDetails = new ProblemDetails
                    {
                        Title = validationError.Title,
                        Type = validationError.Type,
                        Status = validationError.Status,
                        Detail = validationError.Detail,
                        Extensions = { ["invalidParams"] = validationError.InvalidParams },
                    };

                    context.Response.StatusCode = validationError.Status;
                    context.Response.ContentType = "application/problem+json";
                    await context.Response.WriteAsJsonAsync(problemDetails);
                    return;
                }

                // Fallback 500
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(
                    new ProblemDetails { Title = "Internal Server Error" }
                );
            });
        });
    }
}
