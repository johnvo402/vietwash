using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using AuthService.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Domain.Entities;

public class UserActivity
{
    [Key]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public User? User { get; set; }

    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
    public string ActivityType { get; set; } = ActivityTypes.Login;
    public string LoginType { get; set; } = "JWT";
    public string? IpAddress { get; set; }
    public string? DeviceId { get; set; }
    public string? Browser { get; set; }
    public string? DeviceName { get; set; }
    public string? Location { get; set; }
    public bool IsRevoked { get; set; } = false;
}

