using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Micro.Shared.Domain;

namespace AuthService.Domain.Permissions;
public class Permission : BaseAuditableEntity
{
    public string PermissionKey { get; set; } = null!;
    public string Description { get; set; } = null!;

}