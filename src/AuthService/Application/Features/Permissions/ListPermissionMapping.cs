using Application.Features.Common.Projections.Users;
using AutoMapper;
using Domain.Aggregates.Roles;


namespace Application.Features.Permissions
{
    public class ListPermissionMapping : Profile
    {
        public ListPermissionMapping()
        {
            CreateMap<Permission, ListPermissionResponse>();
        }
    }
}
