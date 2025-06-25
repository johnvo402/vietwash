using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.Login;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string? Email { get; set; }

    public string? Password { get; set; }
}
