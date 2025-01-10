using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AuthService.Domain.UserActivities;
using AuthService.Domain.UserRoles;
using AuthService.Domain.Users.Events;
using ErrorOr;
using Micro.Shared.Domain;

namespace AuthService.Domain.Users.Entity;
public class User : BaseAuditableEntity
{
    public string Email { get; set; } = null!;
    public string? Avatar { get; set; }
    public string Password { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
    [DefaultValue("DOAN")]
    public string? OrgId { get; set; } = default;
    public List<UserActivity>? UserActivities { get; set; }
    public ICollection<UserRole>? UserRoles { get; set; }

    public ErrorOr<Success> UpdateLastLogin()
    {
        LastLogin = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserLoggedInEvent(Id.ToString()));
        return Result.Success;
    }

    public void UpdateAuditFields(string updatedBy)
    {
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public ErrorOr<Success> SetOrgId(string orgId)
    {
        if (string.IsNullOrWhiteSpace(orgId))
            return Error.Validation("OrgId cannot be null or empty", nameof(orgId));

        OrgId = orgId;
        return Result.Success;
    }

}
