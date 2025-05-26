using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Update;

public class UpdateUserMapping : Profile
{
    public UpdateUserMapping()
    {
        CreateMap<UpdateUser, User>();
        CreateMap<User, UpdateUserResponse>().IncludeBase<User, UserDetailProjection>();
    }
}
