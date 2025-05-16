using Mediator;

namespace Application.Features.Accounts.Commands.Token;

public class RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public string? RefreshToken { get; set; }
}
