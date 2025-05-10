using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Users;
using System;

namespace Application.Features.Users.Commands.Create;

public class CreateUserMapping : Profile
{
    public CreateUserMapping()
    {
        

        CreateMap<CreateUserCommand, User>()
            .AfterMap(
                (src, dest) =>
                {
                    dest.SetPassword(HashPassword(src.Password));
                }
            );

        CreateMap<User, CreateUserResponse>().IncludeBase<User, UserDetailProjection>();
        CreateMap<User, CreateUserCommand>()
            .ForMember(dest => dest.Password, opt => opt.Ignore());
    }
}
