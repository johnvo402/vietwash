using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.Application.Common.Interfaces.Services.Token;
using JohnChum.SharedKernel.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Application.Common.Auth;

public class AuthorizeHandler(IServiceProvider serviceProvider, IHttpContextAccessor _httpContextAccessor)
    : AuthorizationHandler<AuthorizationRequirement>
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthorizationRequirement requirement
    )
    {
        using var scope = serviceProvider.CreateScope();
        IRedisCacheService cache =
            scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
        ITokenSecurityService tokenSecurity = scope.ServiceProvider.GetRequiredService<ITokenSecurityService>();

        string? userId = null;

        var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorizationHeader["Bearer ".Length..].Trim();
            var decodeToken = tokenSecurity.DecodeToken(token);
            if (decodeToken == null || decodeToken.ExpiredTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds() || decodeToken.TokenType != "access")
            {
                context.Fail(new AuthorizationFailureReason(this, "Token invalid or expired."));
                return;
            }
            userId = decodeToken.Sub;
        }

        if (userId == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is UnAuthenticated"));
            return;
        }


        try
        {
            string requirementJson = requirement.Requirement();
            if (string.IsNullOrEmpty(requirementJson))
            {
                context.Succeed(requirement);
                return;
            }

            AuthorizeModel? authorizeModel = SerializerExtension
                .Deserialize<AuthorizeModel>(requirementJson)
                .Object;

            //if (
            //    authorizeModel == null
            //    || (authorizeModel?.Permissions?.Count == 0 && authorizeModel?.Roles?.Count == 0)
            //)
            //{
            //    context.Succeed(requirement);
            //    return;
            //}
            var user = await cache.Database.StringGetAsync(userId);
            if (string.IsNullOrEmpty(user))
            {
                context.Fail();
                return;
            }
            var result = SerializerExtension.Deserialize<UserAuth>(user!);
            //if (authorizeModel?.Roles?.Count > 0 && authorizeModel.Permissions?.Count > 0)
            //{

            //    SuccessOrFailiureHandler(
            //        context,
            //        requirement,
            //        await HasClaimsAndRoleInUserAsync(
            //            authorizeModel.Roles,
            //            authorizeModel.Permissions,
            //            result.Object!
            //        )
            //    );
            //    return;
            //}

            if (authorizeModel?.Roles?.Count > 0)
            {
                SuccessOrFailiureHandler(
                    context,
                    requirement,
                    await HasRolesInUserAsync(result.Object!, authorizeModel.Roles)
                );

                return;
            }

            //if (authorizeModel?.Permissions?.Count > 0)
            //{
            //    SuccessOrFailiureHandler(
            //        context,
            //        requirement,
            //        await HasClaimsInUserAsync(
            //           result.Object!,
            //            authorizeModel.Permissions
            //        )
            //    );

            //    return;
            //}
        }
        catch (JsonException)
        {
            // Log error if needed
            context.Fail(new AuthorizationFailureReason(this, "Invalid authorization requirement format"));
            return;
        }

        await Task.CompletedTask;
    }

    private static void SuccessOrFailiureHandler(
        AuthorizationHandlerContext context,
        AuthorizationRequirement requirement,
        bool isSuccess = false
    )
    {
        if (!isSuccess)
        {
            context.Fail();
            return;
        }

        context.Succeed(requirement);
    }


    private async Task<bool> HasRolesInUserAsync(UserAuth user, IEnumerable<string> roleNames) =>
        await Task.FromResult(roleNames.Contains(user.Role!));



}
