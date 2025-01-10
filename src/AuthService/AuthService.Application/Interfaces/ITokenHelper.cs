
using AuthService.Domain.ValueObjects;

namespace AuthService.Application.Interfaces;

public interface ITokenHelper
{
    (string accessToken, string time) GenerateAccessToken(string id,
         string displayName,
         string email,
         List<string> permissions,
         List<string> roles,
         string OrgId);
    string GenerateRefreshToken();
    bool ValidateAccessToken(string token);
}