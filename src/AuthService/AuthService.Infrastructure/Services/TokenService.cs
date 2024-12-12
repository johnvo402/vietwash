using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public TokenService(
        UserManager<User> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public (string accessToken, DateTime expiresAt) GenerateAccessToken(User user, string[] roles)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user), "User cannot be null");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key is not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured"),
            audience: _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured"),
            claims: claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), DateTime.Now.AddMinutes(10));
    }

    public async Task<string> GenerateRefreshToken()
    {


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key is not configured")));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured"),
            audience: _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured"),
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ??
            throw new InvalidOperationException("JWT Key is not configured"));

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
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

    public async Task<bool> RevokeRefreshToken(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

}