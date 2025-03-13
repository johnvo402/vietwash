using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Features.Users.Commands.Login;

public class LoginUserCommand : IRequest<LoginUserResponse>
{
    public string? Username { get; set; }

    public string? Password { get; set; }

    public JwkModel? PublicKey { get; set; }
}
