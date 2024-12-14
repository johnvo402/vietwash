using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class User : IdentityUser
{
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public string? OrgId { get; set; }
    public List<UserActivity>? UserActivities { get; set; }

}
