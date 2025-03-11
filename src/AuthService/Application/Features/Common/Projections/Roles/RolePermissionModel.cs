namespace Application.Features.Common.Projections.Roles;

public class RolePermissionModel
{
    public Ulid? Id { get; set; }

    public Ulid? PermissionId { get; set; }

    public PermissionModel? Permission { get; set; }

}
