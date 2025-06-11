using Application.Features.Common.Projections.Accounts;
using Domain.Aggregates.Accounts.Enums;
using Mediator;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileCommand : AccountModel, IRequest<UpdateAccountProfileResponse>
{
    public Gender? Gender { get; set; }

    public List<AccountContactProjection>? AccountContacts { get; set; }
};
