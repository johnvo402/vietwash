using Domain.Aggregates.Users;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Roles;

public class Role : DefaultEntity
{
    public string? Guard { get; set; }

    public string? Description { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<User>? Users { get; set; } = [];

    public ICollection<RolePermission>? RolePermissions { get; set; } = [];
}
