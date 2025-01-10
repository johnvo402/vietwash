using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AuthService.Application.Interfaces;
using AuthService.Application.Auth.Commands.Login;

using AuthService.Domain.Users.Entity;
using Micro.Shared.Application.Interface;
using Micro.Shared.Infrastructure.Services;
using AuthService.Infrastructure.Services;
using ProductService.API.Extensions;
using Microsoft.AspNetCore.Http;
using Ardalis.GuardClauses;
using AuthService.Infrastructure.Users.Repositories;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Micro.Shared.Infrastructure.Interceptors;
using AuthService.Application.EventHandler;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using Microsoft.Data.SqlClient;
using Micro.Shared.QueryServices;

namespace AuthService.Infrastructure
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
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddSingleton<ITokenHelper, TokenHelper>();
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


            services.AddDbContext<AuthDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlServer(connectionString);
            });
            services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(connectionString));

            services.AddScoped<IUserActivityRepo, UserActivityRepo>();
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IRoleRepo, RoleRepo>();
            services.AddScoped<IPermissionRepo, PermissionRepo>();


            return services;
        }

        private static IServiceCollection AddOData(this IServiceCollection services)
        {
            services.AddOData();
            return services;
        }


    }
}
