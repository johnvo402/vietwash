using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace ProductService.API.Extensions;

public static class ApiVersioningExtensions
{
    public static IServiceCollection AddApiVersioningConfig(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // version default
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = ApiVersion.Parse("1.0");

            // check version from client
            options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");

            options.ReportApiVersions = true;
        });

        return services;
    }
}