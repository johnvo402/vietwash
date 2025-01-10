using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Micro.Shared.Infrastructure.Security.PolicyEnforcer;
using Micro.Shared.Infrastructure.Security;
using Micro.Shared.Application.Interface;
using Micro.Shared.Infrastructure.Security.JwtToken;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ProductService.API.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuthorizationShared(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICurrentUser, CurrentUserProvider>();
        services.TryAddSingleton<IPolicyEnforcer, PolicyEnforcer>();
        services.AddAuthorization();

        return services;
    }
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Section));

        services
            .ConfigureOptions<JwtBearerTokenValidationConfiguration>()
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer();

        return services;
    }
    public static IApplicationBuilder UseAuthenticationConfig(
       this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}