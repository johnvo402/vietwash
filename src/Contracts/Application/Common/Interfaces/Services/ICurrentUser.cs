using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Application.Common.Interfaces.Services;

public interface ICurrentAccount
{
    public long? Id { get; }

    public string? ClientIp { get; }

    void SetClientIp(HttpContext httpContext);

    void SetClaimPrinciple(ClaimsPrincipal user);
}
