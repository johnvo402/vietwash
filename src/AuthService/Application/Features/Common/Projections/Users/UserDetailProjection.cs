using Application.Features.Common.Projections.Roles;

namespace Application.Features.Common.Projections.Users;

public class UserDetailProjection : UserProjection
{
    public RoleDetailProjection? Role { get; set; }
}
