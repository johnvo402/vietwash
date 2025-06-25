using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.Logout;

public class LogoutCommand : IRequest<Result<LogoutResponse>>
{
    public string Token { get; set; } = default!;
}
