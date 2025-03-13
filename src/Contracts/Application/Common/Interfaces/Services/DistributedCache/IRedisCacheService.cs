using StackExchange.Redis;

namespace Application.Common.Interfaces.Services.DistributedCache;

public interface IRedisCacheService
{
    IDatabase Database { get; }
    Task<T> GetOrSetAsync<T>(string key,
        Func<Task<T>> task,
        TimeSpan expiry);
}
