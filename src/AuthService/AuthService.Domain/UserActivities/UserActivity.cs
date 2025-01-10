using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using AuthService.Domain.Users.Entity;
using AuthService.Domain.ValueObjects;
using Micro.Shared.Domain;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.UserActivities;
public class UserActivity : BaseAuditableEntity
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public DateTimeOffset ActivityDate { get; set; }
    public string ActivityType { get; set; } = default!;
    public string? LoginType { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? Browser { get; set; }
    public string? DeviceName { get; set; }
    public string? Location { get; set; }
    public bool IsRevoked { get; set; } = false;
}

