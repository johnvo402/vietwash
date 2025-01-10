using AuthService.Application.Users.Commands.UpdateUserCommands;
using AuthService.Domain.Users.Entity;
using AutoMapper;

namespace AuthService.Application;
public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<UserUpdateDto, User>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); ;
    }
}