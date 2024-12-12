using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class Role : IdentityRole
{
    public string? OrgId { get; set; }
}
