using Mediator;

namespace Application.Features.Accounts.Commands.RequestResetPassword;

public record RequestResetAccountPasswordCommand(string Email) : IRequest;
