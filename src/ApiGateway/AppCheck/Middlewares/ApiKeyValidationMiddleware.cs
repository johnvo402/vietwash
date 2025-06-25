using System.Net;
using System.Text.Json;
using ApiGateway.AppCheck.Extensions;
using ApiGateway.AppCheck.Models;
using Microsoft.Extensions.Options;
using Wangkanai.Detection.Services;

namespace ApiGateway.AppCheck.Middlewares;

public class ApiKeyValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiSettings _apiSettings;

    public ApiKeyValidationMiddleware(RequestDelegate next, IOptions<ApiSettings> apiSettings)
    {
        _next = next;
        _apiSettings = apiSettings.Value;
    }

    public async Task InvokeAsync(HttpContext context, IDetectionService detectionService)
    {
        var request = context.Request;

        // Lấy header
        var apiKey = request.Headers["x-api-key"].FirstOrDefault();
        var platform = request.Headers["platform"].FirstOrDefault();
        var origin = request.Headers["origin"].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(platform))
        {
            await ReturnUnauthorized(context, "Missing ApiKey or Platform header.");
            return;
        }

        if (detectionService.IsWeb())
        {
            // Web
            if (apiKey != _apiSettings.Web?.ApiKey || platform != _apiSettings.Web?.Platform)
            {
                await ReturnUnauthorized(context, "Invalid ApiKey or Platform for Web.");
                return;
            }

            if (
                _apiSettings.Web.Origin != null
                && !_apiSettings.Web.Origin.Contains(origin ?? string.Empty)
            )
            {
                await ReturnUnauthorized(context, "Origin not allowed.");
                return;
            }
        }
        else if (detectionService.IsMobileOrTablet())
        {
            // Mobile
            if (apiKey != _apiSettings.Mobile?.ApiKey || platform != _apiSettings.Mobile?.Platform)
            {
                await ReturnUnauthorized(context, "Invalid ApiKey or Platform for Mobile.");
                return;
            }
        }
        else
        {
            await ReturnUnauthorized(context, "Unknown device.");
            return;
        }

        // Pass to next middleware
        await _next(context);
    }

    private async Task ReturnUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";
        var response = JsonSerializer.Serialize(new { error = message });
        await context.Response.WriteAsync(response);
    }
}
