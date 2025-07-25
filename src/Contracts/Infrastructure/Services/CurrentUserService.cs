using System.Security.Claims;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.Extensions;

namespace Infrastructure.Services;

public class CurrentUserService(IServiceProvider serviceProvider) : ICurrentAccount
{
    public long? Id { get; private set; }

    public string? ClientIp { get; private set; }

    public UserAuth? Session { get; private set; }

    private ClaimsPrincipal user = null!;
    private readonly IServiceProvider serviceProvider = serviceProvider;

    public async Task SetClaimPrinciple(ClaimsPrincipal user)
    {
        this.user = user;
        if (user?.Identity?.IsAuthenticated != true)
            return;
        string? id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(id))
        {
            Id = long.Parse(id);

            using var scope = serviceProvider.CreateScope();
            IRedisCacheService cache =
                scope.ServiceProvider.GetRequiredService<IRedisCacheService>();
            var account = await cache.Database.StringGetAsync(id);
            if (account.HasValue)
            {
                var result = SerializerExtension.Deserialize<UserAuth>(account!);
                Session = result.Object;
            }
            else
            {
                Session = null;
            }
        }
        else
        {
            Id = null;
        }
    }

    public void SetClientIp(HttpContext httpContext)
    {
        ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
