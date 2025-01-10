using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Micro.Shared.Extensions
{
    public static class CorsExtentions
    {

        public static IServiceCollection AddSharedCors(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();

                });
            });

            return services;
        }
        public static IApplicationBuilder UseSharedCors(this IApplicationBuilder app, string policyName)
        {
            app.UseCors(policyName);
            return app;
        }
    }
}