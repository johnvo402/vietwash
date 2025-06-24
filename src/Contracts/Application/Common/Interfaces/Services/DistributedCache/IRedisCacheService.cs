using StackExchange.Redis;

namespace Application.Common.Interfaces.Services.DistributedCache;

public interface IRedisCacheService
{
    IDatabase Database { get; }
    public T? GetOrSet<T>(string key, Func<T> func, TimeSpan? expiry = null);

    public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> task, TimeSpan? expiry = null);
}
