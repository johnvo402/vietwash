using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Accounts.Commands.ResetPassword;

public record ResetAccountPasswordCommand(string Token, long AccountId, string Password)
    : IRequest<Result>;
