using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserMapping : Profile
{
    public UpdateUserMapping()
    {
        CreateMap<UpdateUser, User>()
            .ForMember(dest => dest.Role, opt => opt.Ignore());
        CreateMap<User, UpdateUserResponse>().IncludeBase<User, UserDetailProjection>();
    }
}
