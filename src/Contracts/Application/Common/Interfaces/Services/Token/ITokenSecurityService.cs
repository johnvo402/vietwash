using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;

namespace Contracts.Application.Common.Interfaces.Services.Token
{
    public interface ITokenSecurityService
    {
        Task AddToBlacklistAsync(string token, TimeSpan expiry);
        Task AddSessionUserAsync(string userId,string userData, TimeSpan expiry);
        Task<bool> IsTokenBlacklistedAsync(string token);
        DecodeTokenResponse DecodeToken(string token);
        Task<bool> ExistsNonceAsync(string nonce);
        Task StoreNonceAsync(string nonce);

    }
}
