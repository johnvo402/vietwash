using System.Text;
using Application.Common.Exceptions;
using Contracts.Application.Common.Exceptions;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Common.Messages;
using Contracts.Infrastructure.Services.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Token;

public static class JwtRegisterExtension
{
    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        services.Configure<JwtSettings>(
            config.GetSection($"SecuritySettings:{nameof(JwtSettings)}")
        );

        var jwtSettings = config
            .GetSection($"SecuritySettings:{nameof(JwtSettings)}")
            .Get<JwtSettings>();

        services.AddSingleton<ITokenSecurityService, TokenSecurityService>();

        return services
            .AddAuthentication(authentication =>
            {
                authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(bearer =>
            {
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(jwtSettings!.SecretKey!)
                    ),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                bearer.IncludeErrorDetails = true;
                bearer.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        return TokenErrorExtension.UnauthorizedException(
                            context,
                            !context.Response.HasStarted
                                ? new UnauthorizedError(Message.UNAUTHORIZED)
                                : new UnauthorizedError(Message.TOKEN_EXPIRED)
                        );
                    },
                    OnForbidden = context =>
                        TokenErrorExtension.ForbiddenException(
                            context,
                            new ForbiddenError(Message.FORBIDDEN)
                        ),
                };
            })
            .Services;
    }
}
