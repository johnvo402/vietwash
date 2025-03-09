using Application.Common.Interfaces.Services.DistributedCache;
using Contracts.Application.Common.Interfaces.Services.Token;
using Microsoft.Extensions.Caching.Distributed;


namespace Contracts.Infrastructure.Services.Token
{
    public class BlacklistTokenService : IBlacklistTokenService
    {
        private readonly IRedisCacheService _cache;
        private const string BlacklistPrefix = "blacklist_token_";

        public BlacklistTokenService(IRedisCacheService cache)
        {
            _cache = cache;
        }

        public async Task AddToBlacklistAsync(string token, TimeSpan expiry)
        {
            string key = $"{BlacklistPrefix}{token}";
            await _cache.Database.StringSetAsync(key, "blacklisted", expiry);
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            string key = $"{BlacklistPrefix}{token}";
            return await _cache.Database.KeyExistsAsync(key);
        }
    }
}
