using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

using AuthService.Domain.Users.Entity;
using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Roles;
using AuthService.Domain.RolePermissions;
using System.Text.Json.Serialization;

namespace AuthService.Domain.UserRoles;

[PrimaryKey(nameof(UserId), nameof(RoleId))]
public class UserRole
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;

    
}
