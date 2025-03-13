using Application.Common.Auth;
using Application.Features.Common.Projections.Users;
using Application.Features.Users.Queries.Detail;
using AutoMapper;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Commands.Login;

public class LoginUserMapping : Profile
{
    public LoginUserMapping()
    {
        CreateMap<User, GetUserDetailResponse>().IncludeBase<User, UserDetailProjection>();

    }
}
