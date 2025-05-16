using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileMapping : Profile
{
    public UpdateAccountProfileMapping()
    {
        CreateMap<UpdateAccountProfileCommand, Account>();
        CreateMap<Account, UpdateAccountProfileResponse>();
    }
}
