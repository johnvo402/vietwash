namespace Infrastructure.Services.Token;

public class JwtSettings
{
    public string? SecretKey { get; set; }

    public string? Issuer { get; set; }
    public string? Audience { get; set; }

    public string? ExpireTimeAccessToken { get; set; }

    public string? ExpireTimeRefreshToken { get; set; }
}
