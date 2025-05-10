using Mediator;

namespace Application.Features.Users.Commands.ResetPassword;

public record ResetUserPasswordCommand(string Token, long UserId, string Password) : IRequest;
