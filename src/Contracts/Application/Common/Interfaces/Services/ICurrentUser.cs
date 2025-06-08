using System.Security.Claims;
using Application.Common.Auth;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services;

public interface ICurrentAccount
{
    public long? Id { get; }

    public string? ClientIp { get; }

    public UserAuth? Session { get; }

    void SetClientIp(HttpContext httpContext);
    Task SetClaimPrinciple(ClaimsPrincipal user);
}
