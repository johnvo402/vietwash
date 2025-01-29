using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Application;
using Micro.Shared.Extensions;
using ProductService.API.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Infrastructure;

namespace ProductService.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddSharedSwagger("Product Service API");
            services.AddApiVersioningConfig();
            services.AddDataProtectionConfig(builder.Configuration);
            services.AddApplication();
            services.AddInfrastructure(builder.Configuration);

            return services;
        }

        public static IApplicationBuilder UseConfigure(this IApplicationBuilder app)
        {
            // Configure middleware and authentication
            app.UseAuthenticationConfig();

            return app;
        }
    }
}