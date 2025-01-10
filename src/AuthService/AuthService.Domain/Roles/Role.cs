using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AuthService.Domain.RolePermissions;
using AuthService.Domain.UserRoles;
using AuthService.Domain.ValueObjects;
using Micro.Shared.Domain;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Roles;
public class Role : BaseAuditableEntity
{
    public required string RoleName { get; set; }
    public string? OrgId { get; set; }
    public ICollection<RolePermission>? RolePermissions { get; set; }
    public ICollection<UserRole>? UserRoles { get; set; }
}
