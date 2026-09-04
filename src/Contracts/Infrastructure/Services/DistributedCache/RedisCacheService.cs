using Application.Common.Interfaces.Services.DistributedCache;
using Shared.Kernel.Extensions;
using StackExchange.Redis;

namespace Infrastructure.Services.DistributedCache;

public class RedisCacheService(IConnectionMultiplexer multiplexer) : IRedisCacheService
{
    private readonly IDatabase database = multiplexer.GetDatabase();
    public IDatabase Database => database;

    public T? GetOrSet<T>(string key, Func<T> func, TimeSpan? expiry = null)
    {
        RedisValue currentValue = Database.StringGet(key);

        if (currentValue.IsNull)
        {
            T value = func();
            SerializeResult result = SerializerExtension.Serialize(value!);
            _ = Database.StringSet(key, result.StringJson, expiry, when: When.Always);
            return value;
        }

        return SerializerExtension.Deserialize<T>(currentValue.ToString()).Object;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> task, TimeSpan? expiry = null)
    {
        RedisValue currentValue = await Database.StringGetAsync(key);

        if (currentValue.IsNull)
        {
            T value = await task();
            SerializeResult result = SerializerExtension.Serialize(value!);
            _ = await Database.StringSetAsync(key, result.StringJson, expiry, when: When.Always);
            return value;
        }

        return SerializerExtension.Deserialize<T>(currentValue.ToString()).Object;
    }
}
