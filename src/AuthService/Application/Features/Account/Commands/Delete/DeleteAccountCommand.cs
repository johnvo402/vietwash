using Mediator;

namespace Application.Features.Accounts.Commands.Delete;

public record DeleteAccountCommand(long AccountId) : IRequest;
