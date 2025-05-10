using Domain.Aggregates.Users.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Common.Projections.Users;

public class UserModel
{
    public Ulid Id { get; set; }
    public string? Username { get; set; }

    public Gender? Gender { get; set; }

    public UserStatus Status { get; set; }

    public string Role { get; set; }
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? DayOfBirth { get; set; }
    public string? Street { get; set; }
}
