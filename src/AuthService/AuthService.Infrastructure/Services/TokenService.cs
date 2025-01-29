using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.ValueObjects;
using Micro.Shared.Infrastructure.Security.JwtToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class TokenHelper(IOptions<JwtSettings> jwtOptions) : ITokenHelper
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;


    public (string accessToken, string time) GenerateAccessToken(
        string id,
        string displayName,
        string email,
        List<string> permissions,
        List<string> roles,
        string OrgId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Name, displayName),
            new(JwtRegisteredClaimNames.Email, email),
            new("id", id),
            new("org_id", OrgId)
        };
        roles.ForEach(role => claims.Add(new("roles", role.ToString())));
        permissions.ForEach(permission => claims.Add(new("permissions", permission)));
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes);
        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires.ToString());
    }

    public string GenerateRefreshToken(string id)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new("id", id)
        };
        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.TokenExpirationInMinutes + _jwtSettings.TokenExpirationInMinutes / 2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid GetUserIdFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();

        var jwtToken = handler.ReadJwtToken(token);

        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

        return Guid.Parse(userId?.ToString() ?? "");
    }

    public bool ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = validatedToken as JwtSecurityToken;
            if (jwtToken == null || jwtToken.ValidTo < DateTime.UtcNow)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}