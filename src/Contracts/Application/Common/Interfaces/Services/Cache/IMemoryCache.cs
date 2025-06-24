namespace Contracts.Application.Common.Interfaces.Services.Cache;

public interface IMemoryCacheService
{
    public T? GetOrSet<T>(string key, Func<T> func, TimeSpan? expiry = null);

    public Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> task, TimeSpan? expiry = null);
}
