using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Interfaces.Services.Token;
using JohnChum.SharedKernel.Extensions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Application.Common.Auth;

public class AuthorizeHandler(IServiceProvider serviceProvider)
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
        ICurrentUser currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();

        Ulid? userId = currentUser.Id;

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

            if (
                authorizeModel == null
                || (authorizeModel?.Permissions?.Count == 0 && authorizeModel?.Roles?.Count == 0)
            )
            {
                context.Succeed(requirement);
                return;
            }
            var user = await cache.Database.StringGetAsync(userId.Value.ToString());
            if (string.IsNullOrEmpty(user))
            {
                context.Fail();
                return;
            }
            var result = SerializerExtension.Deserialize<UserAuth>(user!);
            if (authorizeModel?.Roles?.Count > 0 && authorizeModel.Permissions?.Count > 0)
            {

                SuccessOrFailiureHandler(
                    context,
                    requirement,
                    await HasClaimsAndRoleInUserAsync(
                        authorizeModel.Roles,
                        authorizeModel.Permissions,
                        result.Object!
                    )
                );
                return;
            }

            if (authorizeModel?.Roles?.Count > 0)
            {
                SuccessOrFailiureHandler(
                    context,
                    requirement,
                    await HasRolesInUserAsync(result.Object!, authorizeModel.Roles)
                );

                return;
            }

            if (authorizeModel?.Permissions?.Count > 0)
            {
                SuccessOrFailiureHandler(
                    context,
                    requirement,
                    await HasClaimsInUserAsync(
                       result.Object!,
                        authorizeModel.Permissions
                    )
                );

                return;
            }
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
    public async Task<bool> HasClaimsAndRoleInUserAsync(
         IEnumerable<string> roles,
        IEnumerable<string> claims,
         UserAuth user
     )
    {
        bool isHaRole = await HasRolesInUserAsync(user, roles);
        bool isHasClaim = await HasClaimsInUserAsync(user, claims);

        return isHaRole && isHasClaim;
    }

    private async Task<bool> HasRolesInUserAsync(UserAuth user, IEnumerable<string> roleNames) =>
        await Task.FromResult(roleNames.Contains(user.Role!));
    private async Task<bool> HasClaimsInUserAsync(
        UserAuth user,
        IEnumerable<string> claims
    )
    {
        return await Task.FromResult(
            user.Permissions!.Any(
                x => claims.Any(
                    claim => x == claim
                )
            )
        );
    }



}
