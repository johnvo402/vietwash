using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AuthService.Domain.Permissions;
using AuthService.Domain.Roles;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Domain.RolePermissions;

[PrimaryKey(nameof(PermissionId), nameof(RoleId))]
public class RolePermission
{
    [Required]
    public Guid PermissionId { get; set; }
    [Required]
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public virtual Permission Permission { get; set; } = default!;

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = default!;
}