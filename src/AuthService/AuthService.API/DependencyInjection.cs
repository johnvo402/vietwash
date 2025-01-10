using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Application;
using AuthService.Infrastructure;
using Micro.Shared.Extensions;
using ProductService.API.Extensions;

namespace AuthService.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddSharedSwagger("Auth Service API");
            services.AddApiVersioningConfig();
            services.AddDataProtectionConfig(builder.Configuration);
            services.AddApplication();
            services.AddInfrastructure(builder.Configuration);
            services.AddSharedCors("AllowAll");

            return services;
        }

        public static IApplicationBuilder UseConfigure(this IApplicationBuilder app)
        {
            app.UseSharedCors("AllowAll");
            // Configure middleware and authentication
            app.UseAuthenticationConfig();

            return app;
        }
    }
}