using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Micro.Shared.Model;

namespace Micro.Shared.Extensions;

public static class HttpContextExtensions
{
    public static UserAccess GetUserAccess(this HttpContext context)
    {
        if (context?.User == null)
            return new UserAccess();

        var userAccess = new UserAccess
        {
            UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            Email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty
        };
        var roles = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value)
            .ToList();
        userAccess.Role = roles;
        // Get all permission claims
        var permissions = context.User.Claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();

        userAccess.Permissions = permissions;

        return userAccess;
    }

    public static UserAccess? GetUserAccessOrDefault(this HttpContext context)
    {
        try
        {
            var userAccess = context.GetUserAccess();
            return string.IsNullOrEmpty(userAccess.UserId) ? null : userAccess;
        }
        catch
        {
            return null;
        }
    }
}