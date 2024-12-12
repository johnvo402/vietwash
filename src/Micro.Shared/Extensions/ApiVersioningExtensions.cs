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
            // Đặt mặc định version cho API
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = ApiVersion.Parse("1.0");

            // Cho phép hỗ trợ truy cập qua nhiều version
            options.ApiVersionReader = new HeaderApiVersionReader("X-API-Version");

            // Dễ dàng kiểm tra phiên bản trong lỗi
            options.ReportApiVersions = true;
        });

        return services;
    }
}