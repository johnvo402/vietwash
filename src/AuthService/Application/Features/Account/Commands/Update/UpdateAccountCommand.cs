using Application.Features.Common.Projections.Accounts;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Accounts.Commands.Update;

public class UpdateAccountCommand : IRequest<UpdateAccountResponse>
{
    [FromRoute(Name = RouterBase.Id)]
    public string AccountId { get; set; } = string.Empty;

    [FromForm]
    public UpdateAccount? Account { get; set; }
}

public class UpdateAccount : AccountModel
{
    public string Role { get; set; }
}
