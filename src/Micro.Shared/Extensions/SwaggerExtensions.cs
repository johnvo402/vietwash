using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Micro.Shared.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSharedSwagger(this IServiceCollection services, string title)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.AddSecurityDefinition("X-API-Version", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "X-API-Version",
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Specify the API version"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                            Type = ReferenceType.SecurityScheme,
                                Id = "X-API-Version"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Laundry Management API, Service: " + title,
                    Version = "v1",
                    Description = "API for managing laundry services",
                    Contact = new OpenApiContact
                    {
                        Name = "John Vo",
                        Email = "thanhthu040202@gmail.com"
                    }
                });
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                // Add JWT Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            return services;
        }

        public static IApplicationBuilder UseSharedSwagger(this IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(
                c =>
                {
                    c.SwaggerEndpoint("swagger/v1/swagger.json", "Laundry Management API");
                    c.RoutePrefix = string.Empty;
                }
            );

            return app;
        }
    }
}