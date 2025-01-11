using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Interfaces;
using Micro.Shared.Application.Interface;
using Micro.Shared.Infrastructure.Services;
using ProductService.API.Extensions;
using Microsoft.AspNetCore.Http;
using Ardalis.GuardClauses;
using ProductService.Infrastructure.Repositories;
using ProductService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Micro.Shared.Infrastructure.Interceptors;
using System.Data;
using Microsoft.Data.SqlClient;
using Micro.Shared.QueryServices;

namespace ProductService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            services
                .AddHttpContextAccessor()
                .AddServices()
                .AddAuthentication(configuration)
                .AddAuthorizationShared()
                .AddPersistence(configuration).AddLogging(); ;

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
            services.AddScoped<IDapperQueryBuilder, DapperQueryBuilder>();
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");


            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 15,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });
            services.AddScoped<IDbConnection>(sp =>
                    new SqlConnection(connectionString));

            services.AddScoped<IProductRepository, ProductRepository>();
            return services;
        }


    }
}
