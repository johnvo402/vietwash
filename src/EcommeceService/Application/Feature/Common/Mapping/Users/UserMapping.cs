using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Users;

namespace Application.Features.Common.Mapping.Users;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, UserProjection>();

        CreateMap<User, UserDetailProjection>();


    }
}
