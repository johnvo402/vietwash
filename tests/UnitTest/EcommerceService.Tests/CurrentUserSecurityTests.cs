using System.Net;
using System.Security.Claims;
using Application.Common.Auth;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.Middlewares;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Kernel.Extensions;
using StackExchange.Redis;

namespace EcommerceService.Tests;

public class CurrentUserSecurityTests
{
    [Fact]
    public void CurrentAccountRegistration_IsScoped()
    {
        ServiceCollection services = new();

        services.AddCurrentAccount();

        ServiceDescriptor descriptor = Assert.Single(
            services,
            item => item.ServiceType == typeof(ICurrentAccount)
        );
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void TwoDependencyInjectionScopes_ResolveDifferentCurrentAccounts()
    {
        ServiceCollection services = new();
        services.AddCurrentAccount();
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();

        ICurrentAccount first = firstScope.ServiceProvider.GetRequiredService<ICurrentAccount>();
        ICurrentAccount second = secondScope.ServiceProvider.GetRequiredService<ICurrentAccount>();

        Assert.NotSame(first, second);
    }

    [Fact]
    public async Task Middleware_AwaitsClaimInitializationBeforeRunningNext()
    {
        DelayedCurrentAccount currentAccount = new();
        bool nextRan = false;
        UserMiddleware middleware = new(_ =>
        {
            Assert.True(currentAccount.InitializationCompleted);
            nextRan = true;
            return Task.CompletedTask;
        });
        DefaultHttpContext context = new() { User = Principal("1") };

        Task invocation = middleware.Invoke(context, currentAccount);
        await currentAccount.InitializationStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.False(nextRan);

        currentAccount.CompleteInitialization();
        await invocation;

        Assert.True(nextRan);
    }

    [Fact]
    public async Task AnonymousInitialization_StartsWithNullIdentity()
    {
        using ServiceProvider provider = BuildCurrentAccountProvider();
        using IServiceScope scope = provider.CreateScope();
        ICurrentAccount currentAccount = scope.ServiceProvider.GetRequiredService<ICurrentAccount>();

        await currentAccount.SetClaimPrinciple(new ClaimsPrincipal(new ClaimsIdentity()));

        Assert.Null(currentAccount.Id);
        Assert.Null(currentAccount.Session);
    }

    [Fact]
    public async Task MalformedOrAnonymousIdentity_ClearsPreviouslyLoadedState()
    {
        using ServiceProvider provider = BuildCurrentAccountProvider();
        using IServiceScope scope = provider.CreateScope();
        ICurrentAccount currentAccount = scope.ServiceProvider.GetRequiredService<ICurrentAccount>();

        await currentAccount.SetClaimPrinciple(Principal("1"));
        Assert.Equal(1, currentAccount.Id);
        Assert.Equal(1, currentAccount.Session?.Id);

        await currentAccount.SetClaimPrinciple(Principal("malformed"));
        Assert.Null(currentAccount.Id);
        Assert.Null(currentAccount.Session);

        await currentAccount.SetClaimPrinciple(new ClaimsPrincipal(new ClaimsIdentity()));
        Assert.Null(currentAccount.Id);
        Assert.Null(currentAccount.Session);
    }

    [Fact]
    public async Task ConcurrentScopes_DoNotShareUserIdentity()
    {
        using ServiceProvider provider = BuildCurrentAccountProvider();
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();
        ICurrentAccount first = firstScope.ServiceProvider.GetRequiredService<ICurrentAccount>();
        ICurrentAccount second = secondScope.ServiceProvider.GetRequiredService<ICurrentAccount>();

        await Task.WhenAll(
            first.SetClaimPrinciple(Principal("1")),
            second.SetClaimPrinciple(Principal("2"))
        );

        Assert.Equal(1, first.Id);
        Assert.Equal(1, first.Session?.Id);
        Assert.Equal(["1"], first.Session?.Branches);
        Assert.Equal(2, second.Id);
        Assert.Equal(2, second.Session?.Id);
        Assert.Equal(["2"], second.Session?.Branches);
    }

    [Fact]
    public void ClientIp_IsRequestScoped()
    {
        ServiceCollection services = new();
        services.AddCurrentAccount();
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope firstScope = provider.CreateScope();
        using IServiceScope secondScope = provider.CreateScope();
        ICurrentAccount first = firstScope.ServiceProvider.GetRequiredService<ICurrentAccount>();
        ICurrentAccount second = secondScope.ServiceProvider.GetRequiredService<ICurrentAccount>();
        DefaultHttpContext firstContext = new();
        DefaultHttpContext secondContext = new();
        firstContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.1");
        secondContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.2");

        first.SetClientIp(firstContext);
        second.SetClientIp(secondContext);

        Assert.Equal("10.0.0.1", first.ClientIp);
        Assert.Equal("10.0.0.2", second.ClientIp);
    }

    private static ServiceProvider BuildCurrentAccountProvider()
    {
        Mock<IDatabase> database = new(MockBehavior.Strict);
        database
            .Setup(x =>
                x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>())
            )
            .Returns(
                (RedisKey key, CommandFlags _) =>
                {
                    long id = long.Parse(key.ToString());
                    string json = SerializerExtension.Serialize(
                        new UserAuth
                        {
                            Id = id,
                            Role = "STAFF",
                            Branches = [id.ToString()],
                        }
                    ).StringJson;
                    return Task.FromResult((RedisValue)json);
                }
            );
        Mock<IRedisCacheService> cache = new(MockBehavior.Strict);
        cache.SetupGet(x => x.Database).Returns(database.Object);

        ServiceCollection services = new();
        services.AddSingleton(cache.Object);
        services.AddCurrentAccount();
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal Principal(string id) =>
        new(
            new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, id)],
                authenticationType: "test"
            )
        );

    private sealed class DelayedCurrentAccount : ICurrentAccount
    {
        private readonly TaskCompletionSource initializationGate = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public TaskCompletionSource InitializationStarted { get; } = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public long? Id => null;
        public string? ClientIp { get; private set; }
        public UserAuth? Session => null;
        public bool InitializationCompleted { get; private set; }

        public async Task SetClaimPrinciple(ClaimsPrincipal user)
        {
            InitializationStarted.TrySetResult();
            await initializationGate.Task;
            InitializationCompleted = true;
        }

        public void SetClientIp(HttpContext httpContext) =>
            ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();

        public void CompleteInitialization() => initializationGate.TrySetResult();
    }
}
