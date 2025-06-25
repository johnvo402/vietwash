using Application.Common.Interfaces.Services.DistributedCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Infrastructure.Services.DistributedCache;

public static class RedisRegisterExtension
{
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var databaseSettings =
            configuration.GetSection(nameof(RedisDatabaseSettings)).Get<RedisDatabaseSettings>()
            ?? new();

        if (!databaseSettings.IsEnbaled)
            return services;
        var configString =
            $"{databaseSettings.Host}:{databaseSettings.Port},password={databaseSettings.Password},abortConnect=false";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configString)
        );
        // Register settings
        services.Configure<RedisDatabaseSettings>(
            configuration.GetSection(nameof(RedisDatabaseSettings))
        );
        services.Configure<PubSubSettings>(configuration.GetSection(nameof(PubSubSettings)));

        // Register core Redis cache
        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        // Register specific PubSub services
        services.AddSingleton<IPubSubService, PubSubService>();
        services.AddSingleton<PubSubService>();
        services.AddSingleton<DeadLetterPubSubService>();

        // Register PubSub factory
        services.AddSingleton<IPubSubFactory, PubSubFactory>();

        // Optional: tune host options (parallel startup/shutdown)
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });

        return services;
    }
}
