using System.Text;
using Application.Common.Exceptions;
using Contracts.Application.Common.Interfaces.Services.Token;
using Contracts.Infrastructure.Services.Token;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
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
                                ? new UnauthorizedException(Message.UNAUTHORIZED)
                                : new UnauthorizedException(Message.TOKEN_EXPIRED)
                        );
                    },
                    OnForbidden = context =>
                        TokenErrorExtension.ForbiddenException(
                            context,
                            new ForbiddenException(Message.FORBIDDEN)
                        ),
                };
            })
            .Services;
    }
}
