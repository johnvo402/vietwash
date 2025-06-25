using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.Token;

public class RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
{
    public string? RefreshToken { get; set; }
}
