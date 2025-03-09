using Application.Features.Common.Projections.Roles;
using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Roles;
using Domain.Aggregates.Users;
using Domain.Aggregates.Users.Enums;
using Domain.Aggregates.Users.ValueObjects;

namespace Application.Features.Common.Mapping.Users;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, UserProjection>().IncludeMembers(x => x.Address);

        CreateMap<User, UserDetailProjection>()
            .IncludeMembers(x => x.Address)
            .ForMember(
                dest => dest.Role,
                opt => opt.MapFrom(src => src.Role)
            );
        CreateMap<Address, UserProjection>();
        CreateMap<Address, UserDetailProjection>();

        CreateMap<Role, RoleDetailProjection>();
        CreateMap<RoleClaim, RoleClaimDetailProjection>();

    }
}
