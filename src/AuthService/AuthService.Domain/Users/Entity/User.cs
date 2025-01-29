using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AuthService.Domain.Roles;
using AuthService.Domain.UserActivities;
using AuthService.Domain.UserRoles;
using AuthService.Domain.Users.Events;
using ErrorOr;
using Micro.Shared.Domain;
using Utilities;

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
    public string Keywords { get; set; } = null!;

    public ErrorOr<Success> UpdateLastLogin()
    {
        LastLogin = DateTimeOffset.UtcNow;
        AddDomainEvent(new UserLoggedInEvent(Id.ToString()));
        return Result.Success;
    }

    public ErrorOr<Success> AddActivities(List<UserActivity> activities)
    {
        UserActivities ??= new List<UserActivity>();
        try
        {
            UserActivities.AddRange(activities);
            return Result.Success;
        }
        catch (System.Exception e)
        {
            return Error.Failure("Add user activity failure " + e.Message);
        }
    }

    public ErrorOr<Success> AddRoles(List<Role> roles)
    {
        UserRoles ??= new List<UserRole>();
        try
        {
            foreach (var role in roles)
            {
                var userRole = new UserRole
                {
                    UserId = Id,
                    RoleId = role.Id,
                    Role = role
                };

                UserRoles.Add(userRole);
            }
            return Result.Success;
        }
        catch (Exception e)
        {
            return Error.Failure("Add roles failure: " + e.Message);
        }
    }

}
