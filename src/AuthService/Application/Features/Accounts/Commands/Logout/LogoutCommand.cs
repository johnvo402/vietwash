using Mediator;

namespace Application.Features.Accounts.Commands.Logout;

public class LogoutCommand : IRequest<LogoutResponse>
{
    public string Token { get; set; } = default!;
}