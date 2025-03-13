using Mediator;

namespace Application.Features.Users.Commands.Logout;

public class LogoutCommand : IRequest<LogoutResponse>
{
    public string Token { get; set; } = default!;
}