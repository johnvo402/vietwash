using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Users;

namespace Application.Features.Users.Queries.List;

public class ListUserMapping : Profile
{
    public ListUserMapping()
    {
        CreateMap<User, ListUserResponse>().IncludeBase<User, UserProjection>();
    }
}
