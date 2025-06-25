namespace Application.Features.Common.Projections.Accounts;

public class AccountTokenProjection
{
    public string? TokenType { get; set; }

    public long AccessTokenExpiredIn { get; set; }

    public string? Token { get; set; }

    public string? Refresh { get; set; }
}
