using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface ITokenService
{
    (string accessToken, DateTime expiresAt) GenerateAccessToken(User user, string[] roles);
    Task<string> GenerateRefreshToken();
    bool ValidateAccessToken(string token);
    Task<bool> RevokeRefreshToken(string userId);
}