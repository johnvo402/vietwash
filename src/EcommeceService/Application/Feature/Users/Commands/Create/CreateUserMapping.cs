using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Create;

public class CreateUserMapping : Profile
{
    public CreateUserMapping()
    {
        CreateMap<UserModel, User>();
        CreateMap<CreateUserEvent, User>();
        CreateMap<User, UserModel>();
        CreateMap<UserProjection, CreateUserCommand>();
    }
}
