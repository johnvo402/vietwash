using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.ChangePassword;

public class ChangeAccountPasswordCommand : IRequest<Result>
{
    public string? OldPassword { get; set; }

    public string? NewPassword { get; set; }
}
