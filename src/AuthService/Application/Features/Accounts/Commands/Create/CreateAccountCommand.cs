using Application.Features.Common.Projections.Accounts;
using Contracts.ApiWrapper;
using Domain.Aggregates.Accounts.Enums;
using Mediator;

namespace Application.Features.Accounts.Commands.Create;

public class CreateAccountCommand : AccountModel, IRequest<Result<CreateAccountResponse>>
{
    public string? Email { get; set; }

    public string? Password { get; set; }

    public Gender? Gender { get; set; }

    public AccountStatus Status { get; set; }

    public string Role { get; set; } = default!;
    public List<BranchAccountModel>? BranchAccounts { get; set; } = [];
}
