using Application.Features.Common.Projections.Accounts;
using Contracts.ApiWrapper;
using Domain.Aggregates.Accounts.Enums;
using Mediator;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommand
    : AccountModel,
        IRequest<Result<UpdateAccountProfileResponse>>
{
    public string? Email { get; set; }
    public Gender? Gender { get; set; }

    public List<AccountContactProjection>? AccountContacts { get; set; }
};
