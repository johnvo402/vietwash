using Contracts.Application.Common.Interfaces.Services.Cache;
using Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contracts.Infrastructure.Services.Cache.MemoryCache;

public static class MemoryCacheExtension
{
    public static IServiceCollection AddMemoryCaching(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddMemoryCache()
            .Configure<CacheSettings>(options =>
                configuration.GetSection(nameof(CacheSettings)).Bind(options)
            )
            .AddSingleton<IMemoryCacheService, MemoryCacheService>();
    }
}
