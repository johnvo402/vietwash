using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Mediator;

namespace Application.Features.Accounts.Commands.Create;

public class CreateAccountCommand : AccountModel, IRequest<CreateAccountResponse>
{
    public string? Password { get; set; }

    public Gender? Gender { get; set; }

    public AccountStatus Status { get; set; }

    public string Role { get; set; }
}
