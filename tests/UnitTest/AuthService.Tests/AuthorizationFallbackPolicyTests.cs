using Infrastructure.Services.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthService.Tests;

public sealed class AuthorizationFallbackPolicyTests
{
    [Fact]
    public void JwtRegistration_RequiresAuthenticationByDefault()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["SecuritySettings:JwtSettings:SecretKey"] =
                        "test-only-secret-key-with-at-least-32-characters",
                    ["SecuritySettings:JwtSettings:Issuer"] = "test",
                    ["SecuritySettings:JwtSettings:Audience"] = "test",
                }
            )
            .Build();
        var services = new ServiceCollection();

        services.AddJwtAuth(configuration);

        using ServiceProvider provider = services.BuildServiceProvider();
        AuthorizationPolicy? fallback = provider
            .GetRequiredService<IOptions<AuthorizationOptions>>()
            .Value.FallbackPolicy;
        Assert.NotNull(fallback);
        Assert.Contains(
            fallback.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement
        );
    }
}
