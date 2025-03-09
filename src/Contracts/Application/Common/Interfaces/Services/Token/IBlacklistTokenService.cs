namespace Contracts.Application.Common.Interfaces.Services.Token
{
    public interface IBlacklistTokenService
    {
        Task AddToBlacklistAsync(string token, TimeSpan expiry);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}
