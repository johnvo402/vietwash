using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ProductService.API.Extensions
{
    public static class DataProtectionExtension
    {
        public static IServiceCollection AddDataProtectionConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Đảm bảo Singleton cho ConnectionMultiplexer
            services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var redisConnection = configuration.GetConnectionString("REDIS_SERVER")
                           ?? throw new InvalidOperationException("REDIS_SERVER connection string is not configured.");
                    return ConnectionMultiplexer.Connect(redisConnection);
                });

            // Cấu hình Data Protection
            services.AddDataProtection()
                    .PersistKeysToStackExchangeRedis(() =>
                                services.BuildServiceProvider().GetRequiredService<IConnectionMultiplexer>().GetDatabase(),
                                "DataProtection-Keys");
            return services;
        }
    }
}
