using Domain.Aggregates.Enums;
using Domain.Aggregates.Users.Enums;

namespace Application.Features.Common.Projections.Users;

public class UserModel
{
    public long Id { get; set; }
    public Gender? Gender { get; set; }
    public ActivationStatus Status { get; set; }
    public string Role { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvtUrl { get; set; }
    public DateTime? BirthDay { get; set; }
}
