namespace Application.Features.Common.Projections.Roles;

public class RoleDetailProjection : RoleProjection
{
    public ICollection<RolePermissionDetailProjection>? RoleClaims { get; set; }
}
