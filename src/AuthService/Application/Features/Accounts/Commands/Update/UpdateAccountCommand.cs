using Application.Features.Common.Projections.Accounts;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Accounts.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountCommand : IRequest<Result<UpdateAccountResponse>>
{
    [FromRoute(Name = RouterBase.Id)]
    public long AccountId { get; set; }

    [FromBody]
    public UpdateAccount? Account { get; set; }
}

public class UpdateAccount : AccountModel
{
    public string? Email { get; set; }

    public Gender? Gender { get; set; }

    public AccountStatus Status { get; set; }

    public string Role { get; set; } = default!;
    public List<BranchAccountModel>? BranchAccounts { get; set; } = [];
}
