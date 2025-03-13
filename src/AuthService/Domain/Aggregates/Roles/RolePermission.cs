using System.Runtime.InteropServices;
using Domain.Aggregates.Users;
using JohnChum.SharedKernel.Domain.Common;

namespace Domain.Aggregates.Roles;

public class RolePermission : DefaultEntity
{
    public Role? Role { get; set; }

    public Ulid RoleId { get; set; }

    public Permission? Permission { get; set; }

    public Ulid PermissionId { get; set; }


}
