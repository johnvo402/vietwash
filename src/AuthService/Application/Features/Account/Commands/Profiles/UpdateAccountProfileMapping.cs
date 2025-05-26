using AutoMapper;
using Domain.Aggregates.Accounts;

namespace Application.Features.Accounts.Commands.Profiles;

public class UpdateAccountProfileMapping : Profile
{
    public UpdateAccountProfileMapping()
    {
        CreateMap<UpdateAccountProfileCommand, Account>()
    .ForMember(dest => dest.BirthDay, opt =>
        opt.MapFrom(src => src.Birthday.HasValue
            ? DateOnly.FromDateTime(src.Birthday.Value)
            : (DateOnly?)null));
    }
}
